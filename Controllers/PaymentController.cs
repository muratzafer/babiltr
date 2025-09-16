using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EntityLayer.Concrete;
using BusinessLayer.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using babiltr.Models.Payment;

namespace babiltr.Controllers
{
    [AllowAnonymous]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JobTimerService _jobTimerService;
        private readonly IEmailSender _emailSender;
        private readonly PaymentSettings? _paymentSettings;
        public PaymentController(IConfiguration configuration, ApplicationDbContext context = null, IEmailSender emailSender = null, JobTimerService jobTimerService = null, IOptions<PaymentSettings> paymentOptions = null)
        {
            _configuration = configuration;
            _context = context;
            _emailSender = emailSender;
            _jobTimerService = jobTimerService;
            _paymentSettings = paymentOptions?.Value;
        }

        // PayTR İframe Ödeme kodu
        [HttpGet]
        public ActionResult Pay(int jobId)
        {
            // Job tablosundaki ilgili işi al
            var job = _context.Jobs.FirstOrDefault(j => j.JobID == jobId);

            if (job == null)
            {
                return Json(new { status = "error", message = "İş ilanı bulunamadı." });
            }

            // UserId'yi kullanarak ilgili kullanıcıyı al
            var user = _context.Users.FirstOrDefault(u => u.Id == job.UserID);

            if (user == null)
            {
                return Json(new { status = "error", message = "Kullanıcı bilgisi bulunamadı." });
            }

            // User bilgilerini al
            string email = user.Email;
            string name = user.FirstName + " " + user.LastName;
            string address = user.City + "," + user.Country;
            string phone = user.PhoneNumber;

            // appsettings.json'dan PayTR bilgilerini al
            string merchant_id = _configuration["PayTR:MerchantId"];
            string merchant_key = _configuration["PayTR:MerchantKey"];
            string merchant_salt = _configuration["PayTR:MerchantSalt"];

            if (string.IsNullOrWhiteSpace(merchant_id) || string.IsNullOrWhiteSpace(merchant_key) || string.IsNullOrWhiteSpace(merchant_salt))
            {
                return Json(new { status = "error", message = "PayTR API bilgileri (MerchantId/Key/Salt) eksik. Lütfen appsettings.json altındaki PayTR ayarlarını doldurun." });
            }

            // YORUM SATIRI ALTINDA: EKLENEN KOD BUDUR — benzersiz merchant_oid
            string merchant_oid = Guid.NewGuid().ToString("N");
            job.MmerchantOid = merchant_oid;
            _context.SaveChanges();

            // Kullanıcı IP adresini alın
            string user_ip = Request.Headers["X-Forwarded-For"];

            if (string.IsNullOrEmpty(user_ip))
            {
                user_ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            // YORUM SATIRI ALTINDA: EKLENEN KOD BUDUR — tutar hesaplama (B, f, c) + appsettings fallback
            decimal f = job.CustomerSurchargePct > 0 ? job.CustomerSurchargePct : (decimal)(_paymentSettings?.CustomerSurchargeRate ?? 0.0);
            decimal c = job.PlatformCommissionPct > 0 ? job.PlatformCommissionPct : (decimal)(_paymentSettings?.PlatformCommissionRate ?? 0.0);

            var (charged, translator, platform) = PaymentCalculationService.Compute(job.Price, f, c);

            // PayTR kuruş cinsinden tutar
            int paymentAmount = Convert.ToInt32(charged * 100m);

            // YORUM SATIRI ALTINDA: EKLENEN KOD BUDUR — sepet kalemi charged tutar ile
            object[][] user_basket = {
                new object[] { job.Title, charged.ToString("F2"), 1 }
            };

            // Sepeti JSON formatına dönüştürün
            string user_basket_json = Newtonsoft.Json.JsonConvert.SerializeObject(user_basket);
            string user_basket_base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(user_basket_json));

            // Token oluşturma
            string Birlestir = string.Concat(
                merchant_id, user_ip, merchant_oid, email, paymentAmount.ToString(),
                user_basket_base64, "0", "0", "TL", "0", merchant_salt
            );

            if (string.IsNullOrEmpty(user_ip) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(address) ||
                string.IsNullOrEmpty(phone))
            {
                List<string> missingFields = new List<string>();
                if (string.IsNullOrEmpty(user_ip)) missingFields.Add("user_ip");
                if (string.IsNullOrEmpty(email)) missingFields.Add("email");
                if (string.IsNullOrEmpty(name)) missingFields.Add("user_name");
                if (string.IsNullOrEmpty(address)) missingFields.Add("user_address");
                if (string.IsNullOrEmpty(phone)) missingFields.Add("user_phone");

                string errorMessage = "Eksik veya geçersiz Bilgileriniz: " + string.Join(", ", missingFields);
                return RedirectToAction("Error", "Home", new { code = 500, errormasage = errorMessage });
            }
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key)))
            {
                var tokenBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
                string token = Convert.ToBase64String(tokenBytes);

                // YORUM SATIRI ALTINDA: EKLENEN KOD BUDUR — Payments kaydı
                var existingPayment = _context.Set<Payment>().SingleOrDefault(p => p.MerchantOid == merchant_oid);
                if (existingPayment == null)
                {
                    var payment = new Payment
                    {
                        JobId = job.JobID,
                        MerchantOid = merchant_oid,
                        BaseAmount = job.Price,
                        CustomerSurchargePct = job.CustomerSurchargePct,
                        PlatformCommissionPct = job.PlatformCommissionPct,
                        ChargedAmount = charged,
                        TranslatorPayout = translator,
                        PlatformProfit = platform,
                        Status = "PENDING",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Add(payment);
                    _context.SaveChanges();
                }

                // Ödeme için gönderilecek parametreler
                NameValueCollection data = new NameValueCollection
        {
            { "merchant_id", merchant_id },
            { "user_ip", user_ip },
            { "merchant_oid", merchant_oid },
            { "email", email },
            { "payment_amount",paymentAmount.ToString() },
            { "user_basket", user_basket_base64 },
            { "paytr_token", token },
            { "debug_on", "0" },
            { "test_mode", "0" },
            { "no_installment", "0" },
            { "max_installment", "0" },
            { "user_name", name },
            { "user_address", address },
            { "user_phone", phone },
            { "merchant_ok_url", $"https://freelance.babiltr.com/Payment/Success?merchant_oid={merchant_oid}" },
            { "merchant_fail_url", "https://freelance.babiltr.com/Payment/Failure" },
            { "timeout_limit", "30" },
            { "currency", "TL" }
        };

                // PayTR API'ye POST isteği
                using (var client = new WebClient())
                {
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    byte[] response = client.UploadValues("https://www.paytr.com/odeme/api/get-token", "POST", data);
                    string result = Encoding.UTF8.GetString(response);
                    dynamic jsonResponse = JObject.Parse(result);

                    if (jsonResponse.status == "success")
                    {
                        string iframeUrl = "https://www.paytr.com/odeme/guvenli/" + jsonResponse.token;
                        return Redirect(iframeUrl);
                    }
                    else
                    {
                        return Json(new { status = "error", message = jsonResponse.message.ToString() });
                    }
                }
            }
        }

        // Ödeme İşleminden sonra Başarılı Ekranı
        [HttpGet]
        public IActionResult Success(string merchant_oid)
        {
            // Ödeme detaylarını al
            var job = _context.Jobs.FirstOrDefault(j => j.MmerchantOid.ToString() == merchant_oid);

            if (job != null)
            {
                ViewBag.Amount = job.Price.ToString("F2");
                ViewBag.OrderId = merchant_oid;
            }
            else
            {
                ViewBag.Amount = "Bilinmiyor";
                ViewBag.OrderId = "Bilinmiyor";
            }

            return View();
        }

        // Ödeme İşleminden Sonra Ödeme Başarısız Ekranı
        [HttpGet]
        public IActionResult Failure()
        {
            return View();
        }

        // Ödeme İşleminden Sonra PayTR Tarafından Bildirim Aksiyonu
        [HttpPost]
        public async Task<IActionResult> NotifyAsync()
        {
            // POST verilerini al
            string merchantOid = Request.Form["merchant_oid"];
            string status = Request.Form["status"];
            string totalAmount = Request.Form["total_amount"];
            string hash = Request.Form["hash"];

            // YORUM SATIRI ALTINDA: EKLENEN KOD BUDUR — Payment kaydını al ve idempotency
            var paymentRec = await _context.Set<Payment>().SingleOrDefaultAsync(p => p.MerchantOid == merchantOid);
            if (paymentRec != null && paymentRec.Status == "OK")
            {
                // daha önce başarıyla işlendi
                return Ok("OK");
            }

            // PayTR API bilgileri
            string merchantKey = _configuration["PayTR:MerchantKey"];
            string merchantSalt = _configuration["PayTR:MerchantSalt"];

            // Veritabanından iş ve kullanıcı bilgilerini al
            var job = _context.Jobs.Include(j => j.User).FirstOrDefault(j => j.MmerchantOid.ToString() == merchantOid);
            if (job == null)
            {
                return BadRequest("Job not found.");
            }

            // Ödeme durumu için hash doğrulama
            string concatenated = string.Concat(merchantOid, merchantSalt, status, totalAmount);
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchantKey)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
                string calculatedHash = Convert.ToBase64String(hashBytes);

                // Eğer hash eşleşmiyorsa işlem hatalı
                if (hash != calculatedHash)
                {
                    return BadRequest("Hash mismatch.");
                }
            }

            if (status == "success")
            {
                // Ödeme başarılı ise siparişi onayla
                job.Status = "PaymentApproved";
                job.PaymentCompleted = true;
                job.MmerchantOid = merchantOid;
                _context.SaveChanges();

                // YORUM SATIRI ALTINDA: EKLENEN KOD BUDUR — Payment durumunu güncelle ve Payout oluştur
                if (paymentRec != null)
                {
                    paymentRec.Status = "OK";
                    _context.Update(paymentRec);
                    _context.SaveChanges();

                    if (_paymentSettings?.AutoPayout == true)
                    {
                        var payout = new Payout
                        {
                            PaymentId = paymentRec.Id,
                            SubMerchantId = "PENDING_ASSIGN", // TODO: çevirmenin PayTR Alt Üye İşyeri ID'si ile güncellenecek
                            Amount = paymentRec.TranslatorPayout,
                            Status = "PENDING",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Add(payout);
                        _context.SaveChanges();

                        // İsteğe bağlı: burada Hangfire ile background transfer job'ı tetiklenebilir
                        // BackgroundJob.Enqueue&lt;TransferJob&gt;(job =&gt; job.RunAsync(payout.Id));
                    }
                }

                // Timer'ı başlatıyoruz
                _jobTimerService.StartJobTimer(job.JobID);

                // Transfer için timer'ı başlatıyoruz
                _jobTimerService.ScheduleTransferPay(job.JobID);

                // E-posta servisi ile bilgilendirme maili gönder
                var emailSubject = "Ödeme Başarılı - Revizyon Talebi ve Otomatik Tamamlama Hakkında Bilgilendirme";
                var emailBody = $@"
                        <!DOCTYPE html>
                        <html lang='tr'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    background-color: #f4f4f9;
                                    color: #333;
                                    margin: 0;
                                    padding: 0;
                                }}
                                .email-container {{
                                    max-width: 600px;
                                    margin: 20px auto;
                                    padding: 20px;
                                    background-color: #ffffff;
                                    border-radius: 8px;
                                    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                                }}
                                .header {{
                                    text-align: center;
                                    padding: 10px 0;
                                    border-bottom: 2px solid #007bff;
                                }}
                                .content {{
                                    padding: 20px;
                                    line-height: 1.6;
                                }}
                                .footer {{
                                    text-align: center;
                                    margin-top: 20px;
                                    font-size: 0.9em;
                                    color: #888;
                                }}
                                .highlight {{
                                    color: #007bff;
                                    font-weight: bold;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='email-container'>
                                <div class='header'>
                                     <img src=""https://freelance.babiltr.com/Images/Logos/babillogodark.png"" alt=""Babil Freelance"">
                                    <p>Ödeme Bilgilendirme</p>
                                </div>
                                <div class='content'>
                                    <p>Merhaba <strong>Sayın {job.User.FirstName} {job.User.LastName}</strong>,</p>
                                    <p>Ödemeniz başarıyla alınmış ve işleminiz onaylanmıştır. İşte işlem detaylarınız:</p>
                                    <ul>
                                        <li><strong>Ödeme Durumu:</strong> Ödeme Onaylandı</li>
                                        <li><strong>Ödeme Takip Numaranız:</strong> {merchantOid}</li>
                                    </ul>
                                    <p class='highlight'>Hatırlatma:</p>
                                    <p>Ödemenizin tamamlanmasının ardından, 3 gün içinde revizyon talebinde bulunabilirsiniz. Bu süre zarfında revizyon talebi yapmadığınız takdirde, işlem otomatik olarak <strong>Tamamlandı</strong> durumuna geçecektir.</p>
                                    <p>Otomatik tamamlanma tarihi: <strong>{DateTime.Now.AddDays(3):dd MMMM yyyy}</strong></p>
                                    <p>Teşekkür ederiz.</p>
                                    <p>Saygılarımızla,<br>Babil Freelance Ekibi</p>
                                </div>
                                <div class='footer'>
                                    <p>Bu mesaj otomatik olarak oluşturulmuştur, lütfen yanıtlamayınız.</p>
                                    <p><a href='https://freelance.babiltr.com/' target='_blank'>BabilFreelance.com</a></p>
                                </div>
                            </div>
                        </body>
                        </html>";
                await _emailSender.SendEmailAsync(job.User.Email, emailSubject, emailBody);

                return Ok("OK");
            }
            else
            {
                job.Status = "PaymentFailed";
                _context.SaveChanges();
                return Ok("OK");
            }

        }
    }
}
