using babiltr.Models.Jobs;
using babiltr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace babiltr.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context = null)
        {
            _logger = logger;
            _context = context;
        }

        // Ana Sayfa
        [HttpGet]
        public IActionResult MainPage()
        {
            var jobs = _context.Jobs.Where(u => u.Status == "Publish").Select(u => new GetJobViewModel
            {
                UserID = u.UserID,
                JobId = u.JobID,
                Title = u.Title,
                Price = u.Price,
                Country = JobCategoryTranslator.TranslateCountry(u.Country),
                JobType = JobCategoryTranslator.TranslateJobType(u.JobType),
                JobCategory = JobCategoryTranslator.TranslateJobCategory(u.JobCategory),
                PostedDate = u.PostedDate,
                Skills = u.Skills,
            }).ToList();

            return View(jobs);
        }

        // Hata Sayfasý 
        [HttpGet]
        public IActionResult Error(int? code, string errormasage)
        {

            ViewData["ErrorCode"] = code ?? 500;
            ViewData["ErrorMessage"] = errormasage ?? "Bilinmeyen bir hata oluþtu.";
            return View();
        }

    }
}
