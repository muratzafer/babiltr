using babiltr.EntityLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace babiltr.Controllers
{
    [AllowAnonymous]
    public class InfoController : Controller
    {
        private readonly ApplicationDbContext _context;
        public InfoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hakkımızda Sayfası
        [HttpGet]
        public IActionResult AboutUs()
        {
            return View();
        }

        // Ücretlendirme Sayfası
        [HttpGet]
        public IActionResult Pricing()
        {
            return View();
        }

        // Sıkça Sorulan Sorular Sayfası
        [HttpGet]
        public IActionResult SSS()
        {
            return View();
        }

        // Bize Ulaşın Sayfası(get)
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // Bize Ulaşın Sayfası Mesaj içeriğinin gönderilmesi(post)
        [HttpPost]
        public async Task<IActionResult> Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                contact.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                ViewBag.Message = "Mesajınız başarıyla gönderildi!";
                return View();
            }

            return View(contact);
        }

        // Kullanım Koşulları Sayfası
        [HttpGet]
        public IActionResult TermsOfService()
        {
            return View();
        }

        // Gizlilik Politikası Sayfası
        [HttpGet]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
    }
}
