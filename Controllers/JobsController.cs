using babiltr.EntityLayer;
using babiltr.Models.Jobs;
using babiltr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace babiltr.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tüm Getirildiği Sayfa
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetJobs(string keyword, string JobCategory, string jobType, int page = 1, int pageSize = 10)
        {
            var jobsQuery = _context.Jobs.Where(job => job.Status == "Publish").AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                jobsQuery = jobsQuery.Where(job => job.Title.Contains(keyword) || job.Description.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(JobCategory))
            {
                jobsQuery = jobsQuery.Where(job => job.JobCategory.Contains(JobCategory));
            }

            if (!string.IsNullOrEmpty(jobType))
            {
                jobsQuery = jobsQuery.Where(job => job.JobType == jobType);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
            var jobs = jobsQuery.ToList();

            var userApplications = _context.Applications
                .Where(a => a.UserID.ToString() == userId)
                .Select(a => a.JobID)
                .ToList();

            var jobViewModels = jobs.Select(job => new GetJobViewModel
            {
                UserID = job.UserID,
                JobId = job.JobID,
                Title = job.Title,
                Price = job.Price,
                Country = JobCategoryTranslator.TranslateCountry(job.Country),
                JobType = JobCategoryTranslator.TranslateJobType(job.JobType),
                JobCategory = JobCategoryTranslator.TranslateJobCategory(job.JobCategory),
                Skills = job.Skills,
                PostedDate = job.PostedDate,
                IsApplied = userApplications.Contains(job.JobID),
                Translator = user?.Translator ?? false,
            }).ToList();

            var pagedJobs = jobViewModels.ToPagedList(page, pageSize);

            return View(pagedJobs);
        }

        // Kullanıcının Yayınladığı İşleri Getiren Sayfa
        [HttpGet]
        public IActionResult MyCreatedJobs(int page = 1, int pageSize = 10)
        {
            var userid = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            var jobsQuery = _context.Jobs.AsQueryable();

            jobsQuery = jobsQuery.Where(job => job.UserID.ToString() == userid && job.Status == "Publish");

            var jobs = jobsQuery.Select(job => new GetJobViewModel
            {
                JobId = job.JobID,
                Title = job.Title,
                Price = job.Price,
                Country = JobCategoryTranslator.TranslateCountry(job.Country),
                JobType = JobCategoryTranslator.TranslateJobType(job.JobType),
                PostedDate = job.PostedDate
            }).ToList();

            var pagedJobs = jobs.ToPagedList(page, pageSize);
            return View(pagedJobs);
        }

        //Kullanıcının aktif projelerinin getirildiği sayfa
        [HttpGet]
        public IActionResult MyProjects(int page = 1, int pageSize = 10)
        {
            var userid = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            var jobsQuery = _context.Jobs
                .Where(job => job.UserID.ToString() == userid && job.Status != "Publish")
                .Select(job => new GetJobViewModel
                {
                    JobId = job.JobID,
                    Title = job.Title,
                    Price = job.Price,
                    CompletionPercentage = job.CompletionPercentage,
                    PostedDate = job.PostedDate,
                    JobCompleted = job.JobCompleted,
                    PaymentCompleted = job.PaymentCompleted,
                    DeliveryDate = job.DeliveryDate,
                    RevisionCount = job.RevisionCount,
                    TranslatedFileName = job.TranslatedFileName,
                    Status = job.Status,
                    HiredUserID = _context.Applications
                        .Where(application => application.JobID == job.JobID && application.ApplicationStatus == "Approved")
                        .Select(application => application.UserID)
                        .FirstOrDefault(),
                    ChatId = _context.Applications
                        .Where(application => application.JobID == job.JobID && application.ApplicationStatus == "Approved")
                        .Select(application => application.UserID)
                        .Distinct()
                        .Select(hiredUserId => _context.Chats
                            .Where(c =>
                                (c.User1Id.ToString() == userid && c.User2Id == hiredUserId) ||
                                (c.User2Id.ToString() == userid && c.User1Id == hiredUserId))
                            .Select(c => c.Id)
                            .FirstOrDefault())
                        .FirstOrDefault()
                }).ToList();

            var pagedJobs = jobsQuery.ToPagedList(page, pageSize);
            return View(pagedJobs);
        }

        //İş detaylarını getirme (GET)
        [AllowAnonymous]
        [HttpGet]
        public IActionResult JobDetails(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var job = _context.Jobs.FirstOrDefault(j => j.JobID == id);
            if (job == null)
            {

                return NotFound();
            }
            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
            var userApplications = _context.Applications
          .Where(a => a.UserID.ToString() == userId)
          .Select(a => a.JobID)
          .ToList();

            var Job = new GetJobViewModel
            {
                JobId = job.JobID,
                Title = job.Title,
                Description = job.Description,
                JobType = JobCategoryTranslator.TranslateJobType(job.JobType),
                Price = job.Price,
                Skills = job.Skills,
                Experience = job.Experience,
                Country = JobCategoryTranslator.TranslateCountry(job.Country),
                City = JobCategoryTranslator.TranslateCity(job.City),
                UserID = job.UserID,
                PostedDate = job.PostedDate,
                ExpiryDate = job.ExpiryDate,
                JobCategory = job.JobCategory,
                Status = job.Status,
                Translator = user?.Translator ?? false,
                IsApplied = userApplications.Contains(job.JobID)
            };
            return View(Job);
        }

        // Yeni bir iş ekleme formu (GET)
        [HttpGet]
        public IActionResult JobCreate()
        {
            return View();
        }

        // Yeni bir iş ekleme formu (Post)
        [HttpPost]
        public IActionResult JobCreate(CreateJobViewModel model, IFormFile wordFile)
        {
            if (ModelState.IsValid)
            {
                var job = new Jobs
                {
                    Title = model.Title,
                    Description = model.Description,
                    JobType = model.JobType,
                    JobCategory = model.JobCategory,
                    Price = model.Price,
                    Skills = model.Skills,
                    Experience = model.Experience,
                    Country = model.Country,
                    City = model.City,
                    TargetLanguage = model.TargetLanguage,
                    OriginalLanguage = model.OriginalLanguage,
                    Status = "Publish",
                    PostedDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddDays(30),
                    DeliveryDate = model.DeliveryDate,
                    RevisionCount = 1,
                };

                _context.Jobs.Add(job);
                _context.SaveChanges();

                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

                if (userId != null)
                {
                    var user = _context.Users.Find(int.Parse(userId));

                    if (wordFile != null && wordFile.Length > 0)
                    {
                        string ownerName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
                        string fileName = $"{ownerName}_{job.JobID}.docx";
                        string uploadsFolder = Path.Combine("wwwroot", "Files", "OriginalFiles");
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        // Word dosyasını kaydet
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            wordFile.CopyTo(fileStream);
                        }

                        job.PdfFileName = fileName;
                        job.UserID = int.Parse(userId);
                        _context.SaveChanges();

                        // Word dosyasını işleme almak için SeparateSegments aksiyonuna yönlendirme
                        return RedirectToAction("SeparateSegments", "Translation", new { filename = fileName, jobID = job.JobID });
                    }
                }
            }
            return View(model);
        }

        // Yeni bir staj iş ekleme formu (GET)
        [HttpGet]
        public IActionResult CreateInternshipJob()
        {
            return View();
        }

        // Yeni bir staj iş ekleme formu (post)
        [HttpPost]
        public IActionResult CreateInternshipJob(CreateInternshipJobViewModel model)
        {
            if (ModelState.IsValid)
            {
                var internshipJob = new Jobs
                {
                    Title = model.Title,
                    Description = model.Description,
                    JobType = model.JobType,
                    JobCategory = "IN",
                    Price = model.Price,
                    OriginalLanguage = model.OriginalLanguage,
                    TargetLanguage = model.TargetLanguage,
                    Status = "Publish",
                    PostedDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddDays(30),
                    Skills = "",
                    Country = "",
                    City = "",
                    Experience = "",
                };

                _context.Jobs.Add(internshipJob);
                _context.SaveChanges();

                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

                if (userId != null)
                {
                    internshipJob.UserID = int.Parse(userId);
                    _context.SaveChanges();
                }

                return RedirectToAction("JobDetails", new { id = internshipJob.JobID });
            }

            return View(model);
        }

        //İşveren tarafından tamamen tamamlandı ve onaylandı
        [HttpGet]
        public IActionResult JobCompleted(int id)
        {
            var job = _context.Jobs.Find(id);

            if (job == null)
            {
                return NotFound("İş bulunamadı.");
            }

            job.Status = "Completed";
            _context.SaveChanges();
            return RedirectToAction("MainPage", "Home");
        }

        //İş çevirmen tarafından onaylandı
        [HttpGet]
        public IActionResult FinishJob(int id)
        {
            var job = _context.Jobs.Find(id);

            if (job == null)
            {
                return NotFound("İş bulunamadı.");
            }

            job.JobCompleted = true;
            job.Status = "Payment";
            _context.SaveChanges();
            return RedirectToAction("GenerateFile", "Translation", new { jobID = id });
        }

        // İş iptal etme (Çevirmen)
        [HttpGet]
        public IActionResult CancelJob(int id)
        {
            var userid = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            var translations = _context.Translations.Where(t => t.JobId == id).ToList();
            foreach (var translation in translations)
            {
                translation.TranslatedSentence = null;
            }


            var applications = _context.Applications.Where(a => a.JobID == id).ToList();
            foreach (var application in applications)
            {
                application.ApplicationStatus = "Pending";
            }

            var userApplication = applications.FirstOrDefault(a => a.UserID.ToString() == userid);
            if (userApplication != null)
            {
                _context.Applications.Remove(userApplication);
            }

            var job = _context.Jobs.FirstOrDefault(j => j.JobID == id);
            if (job != null)
            {
                job.Status = "Publish";
                job.CompletionPercentage = 0;
            }

            _context.SaveChanges();
            return RedirectToAction("MainPage", "Home");
        }

        // İş iptal etme (ADMİN)
        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult AdminJobDelete(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job == null)
            {
                return NotFound();
            }
            _context.Jobs.Remove(job);
            _context.SaveChanges();
            return RedirectToAction("AllJobs", "Admin");

        }

        // İş iptal etme (ilan sahibi)
        [HttpGet]
        public IActionResult JobDelete(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job == null)
            {
                return NotFound();
            }
            _context.Jobs.Remove(job);
            _context.SaveChanges();
            return RedirectToAction("MainPage", "Home");

        }

        // Revizyon Talebi
        [HttpGet]
        public IActionResult Revision(int id)
        {
            var job = _context.Jobs
                .Include(j => j.Applications)
                .FirstOrDefault(j => j.JobID == id);

            if (job == null)
            {
                return NotFound();
            }

            if (job.RevisionCount == 1)
            {
                job.Status = "Revision";
                job.CompletionPercentage = 0;
                job.JobCompleted = false;
                job.RevisionCount = 0;
            }

            var application = job.Applications.FirstOrDefault(a => a.JobID == id && a.ApplicationStatus == "Approved");

            if (application != null)
            {
                var notification = new Notification
                {
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    UserId = application.UserID,
                    Type = "Application",
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }

            return RedirectToAction("MyProjects", "Jobs");
        }
    }
}
