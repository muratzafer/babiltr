using babiltr.EntityLayer;
using babiltr.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace babiltr.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IEmailSender _emailSender;
        public AccountController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager, IEmailSender emailSender = null)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        // Kullanıcı giriş işlemi (GET)
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Kullanıcı giriş işlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string ReturnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Bu e-posta adresine ait bir kullanıcı bulunmamaktadır.");
            }
            else
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    Response.Cookies.Append("UserName", user.UserName);
                    Response.Cookies.Append("UserId", user.Id.ToString());
                    if (!string.IsNullOrEmpty(user.PhotoUrl))
                    {
                        Response.Cookies.Append("PhotoUrl", user.PhotoUrl);
                    }
                    return RedirectToAction("MainPage", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Şifre veya e-posta hatalıdır.");
                }
            }

            return View(model);
        }

        // Kullanıcı çıkış işlemi 
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var userId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out var temp) ? temp : 0;

            if (userId != 0)
            {
                var readNotifications = await _context.Notifications.Where(n => n.UserId == userId && n.IsRead).ToListAsync();
                _context.Notifications.RemoveRange(readNotifications);
                await _context.SaveChangesAsync();
            }

            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("PhotoUrl");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Kullanıcı kayıt işlemi (GET)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Kullanıcı kayıt işlemi (Post)
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Türkçe karakterleri İngilizceye çevirerek kullanıcı adını oluştur
                var firstNameTranslated = Translator.Translate(model.Firstname);
                var lastNameTranslated = Translator.Translate(model.lastname);

                var userName = $"{firstNameTranslated}_{lastNameTranslated}".ToLower();

                var user = new User
                {
                    UserName = userName,
                    FirstName = model.Firstname,
                    LastName = model.lastname,
                    Email = model.Email,
                    NewPassword = model.Password,
                    KVKKStatus = model.KvkkStatus,
                    RegisterDate = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("user"))
                        await _roleManager.CreateAsync(new Role
                        {
                            Name = "user",
                            NormalizedName = "USER",
                        });

                    await _userManager.AddToRoleAsync(user, "user");
                    var claim = new Claim("UserId", user.Id.ToString());
                    await _userManager.AddClaimAsync(user, claim);

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
                            padding: 20px 0;
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
                        a {{
                            color: #007bff;
                            text-decoration: none;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='header'>
                            <img src='https://freelance.babiltr.com/Images/Logos/babillogodark.png' alt='Babil Freelance'>
                        </div>
                        <div class='content'>
                            <p>Merhaba <strong>{model.Firstname} {model.lastname}</strong>,</p>
                            <p>Babil Freelance platformuna <span style='color: red; font-weight: bold;'>hoş geldiniz</span>!</p>
                            <p>Lütfen <strong>Ana Sayfa &gt; Kullanıcı İkonu &gt; Ayarlar </strong> sayfasına giderek önce kişisel bilgilerinizi doldurunuz. Eğer bir çevirmen iseniz aynı sayfadaki <strong>Ayarlar &gt; İş Bilgileri &gt; Çevirmen misiniz?</strong> adımını işaretleyerek bilgilerinizi tamamlamayı unutmayınız.</p>
                            <p>Soru, öneri ve şikayetleriniz için <a href='https://freelance.babiltr.com/Info/Contact'>iletişim formumuzu</a> doldurabilirsiniz.</p>
                            <p>Teşekkür ederiz, iyi çalışmalar dileriz.</p>
                            <p><strong>Babil Freelance Ekibi</strong></p>
                        </div>
                        <div class='footer'>
                            <p>Bu mesaj otomatik olarak oluşturulmuştur, lütfen yanıtlamayınız.</p>
                            <p><a href='https://freelance.babiltr.com/'>BabilFreelance.com</a></p>
                        </div>
                    </div>
                </body>
                </html>";

                    await _emailSender.SendEmailAsync(user.Email, "Babil Freelance Platformuna Hoş Geldiniz!", emailBody);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // Parola sıfırlama talebi (GET)
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Parola sıfırlama talebi (POST)
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Bu email adresine kayıtlı bir kullanıcı bulunamadı.");
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var baseUrl = "https://freelance.babiltr.com/Account/ResetPassword";
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(user.Email);
            var resetLink = $"{baseUrl}?token={encodedToken}&email={encodedEmail}";

            var fullName = user.UserName.Split('_')[0];
            var lastName = user.UserName.Split('_')[1].ToUpper();

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
                                padding: 20px 0;
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
                            .button {{
                                display: inline-block;
                                padding: 10px 20px;
                                background-color: #007bff;
                                color: white; /* Buton yazı rengi beyaz */
                                font-size: 16px;
                                font-weight: bold;
                                text-decoration: none;
                                border-radius: 5px;
                                margin-top: 20px;
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
                                <p>Şifre Sıfırlama Talebi</p>
                            </div>
                            <div class='content'>
                                <p>Merhaba <strong>{char.ToUpper(fullName[0]) + fullName.Substring(1)} {lastName}</strong>,</p>
                                <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıyı kullanabilirsiniz:</p>
                                <a href='{resetLink}' class='button'>Şifreyi Sıfırla</a>
                                <p>Bağlantıya tıkladıktan sonra yeni şifrenizi belirleyebilirsiniz.</p>
                                <p>Bu isteği siz yapmadıysanız, lütfen bu e-postayı dikkate almayın.</p>
                                <p>Teşekkür ederiz,<br>BabilFreelance Ekibi</p>
                            </div>
                            <div class='footer'>
                                <p>Bu mesaj otomatik olarak oluşturulmuştur, lütfen yanıtlamayınız.</p>
                                <p><a href='https://freelance.babiltr.com/' target='_blank'>BabilFreelance.com</a></p>
                            </div>
                        </div>
                    </body>
                    </html>";

            await _emailSender.SendEmailAsync(user.Email, "Parola Sıfırlama Bağlantısı", emailBody);

            return Redirect("Login");
        }

        // Parola sıfırlama işlemi (GET)
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel { Token = token, Email = email, NewPassword = "", ConfirmPassword = "" };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Geçersiz sıfırlama isteği.");
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                user.OldPassword = user.PasswordHash;
                user.NewPassword = _userManager.PasswordHasher.HashPassword(user, model.NewPassword);
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        // Kullanıcı kayıt işlemi (GET)
        [HttpGet]
        public IActionResult RegisterAdmin()
        {
            return View();
        }

        // Admin kayıt işlemi (POST)
        [HttpPost]
        public async Task<IActionResult> RegisterAdmin(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userName = $"{model.Firstname}.{model.lastname}".ToLower();

                var user = new User
                {
                    UserName = userName,
                    FirstName = model.Firstname,
                    LastName = model.lastname,
                    Email = model.Email,
                    NewPassword = model.Password,
                    KVKKStatus = model.KvkkStatus,
                    RegisterDate = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("admin"))
                    {
                        await _roleManager.CreateAsync(new Role
                        {
                            Name = "admin",
                            NormalizedName = "ADMIN",
                        });
                    }

                    await _userManager.AddToRoleAsync(user, "admin");
                    var claim = new Claim("UserId", user.Id.ToString());
                    await _userManager.AddClaimAsync(user, claim);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Admin");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
    }
}
