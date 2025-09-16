using babiltr.EntityLayer;
using babiltr.Models.Applications;
using babiltr.Models.USER;
using babiltr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace babiltr.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ApplicationsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment = null)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // İlan başvuru yapma (Get)
        [HttpGet]
        public IActionResult JobApply(int id)
        {
            var userid = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userid);
            var job = _context.Jobs.FirstOrDefault(j => j.JobID == id);

            if (user == null)
            {
                return NotFound();
            }
            var ModelUser = new JobApplyUserViewModel
            {
                JobID = id,
                JobTitle = job.Title,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Occupation = user.Occupation,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CvOrResumeUrl = user.CvOrResumeUrl,
                Introduction = user.Introduction,
            };
            return View(ModelUser);
        }

        //İlan başvuru yapma(POST)
        [HttpPost]
        public IActionResult JobApply(JobApplyUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userid = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var existingUser = _context.Users.FirstOrDefault(u => u.Id.ToString() == userid);

                if (existingUser != null)
                {
                    existingUser.FirstName = model.FirstName;
                    existingUser.LastName = model.LastName;
                    existingUser.Email = model.Email;
                    existingUser.PhoneNumber = model.PhoneNumber;
                    existingUser.Occupation = model.Occupation;
                    if (model.Introduction != null)
                    {
                        existingUser.Introduction = model.Introduction;
                    }

                    if (model.CvOrResumeFile != null)
                    {
                        string firstName = existingUser.FirstName;
                        string lastName = existingUser.LastName;
                        int userId = existingUser.Id;

                        string fileExtension = Path.GetExtension(model.CvOrResumeFile.FileName);

                        string uniqueFileName = $"{firstName}_{lastName}_{userId}_cv{fileExtension}";

                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "CV");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            model.CvOrResumeFile.CopyTo(fileStream);
                        }

                        existingUser.CvOrResumeUrl = uniqueFileName;
                        _context.Users.Update(existingUser);
                        _context.SaveChanges();
                    }
                    _context.Users.Update(existingUser);
                    _context.SaveChanges();
                }
                var job = _context.Jobs.FirstOrDefault(j => j.JobID == model.JobID);

                if (job == null)
                {
                    return NotFound();
                }

                var application = new Application
                {
                    JobID = model.JobID,
                    UserID = int.Parse(userid),
                    ApplicationDate = DateTime.Now,
                    ApplicationStatus = "Pending"
                };

                _context.Applications.Add(application);
                _context.SaveChanges();

                return RedirectToAction("UserApplications", "Applications");
            }
            return View(model);
        }

        // Başvuru geri alma(Admin)
        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult AdminApplicationDelete(int id)
        {
            var application = _context.Applications.FirstOrDefault(a => a.ApplicationID == id);

            if (application == null)
            {
                return NotFound();
            }

            _context.Applications.Remove(application);
            _context.SaveChanges();

            return RedirectToAction("AllApplications", "Admin");
        }

        // Başvuru geri alma(ÇEVİRMEN)
        [HttpGet]
        public IActionResult JobApplicationDelete(int id)
        {
            var application = _context.Applications.FirstOrDefault(a => a.ApplicationID == id);

            if (application == null)
            {
                return NotFound();
            }

            _context.Applications.Remove(application);
            _context.SaveChanges();

            return RedirectToAction("UserApplications", "Applications");
        }

        //Kullanıcının işlerini ve başvurularını gördüğü sayfa
        [HttpGet]
        public IActionResult UserApplications()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            var applications = _context.Applications
                .Where(a => a.UserID.ToString() == userId)
                .Select(a => new
                {
                    a.ApplicationID,
                    a.JobID,
                    a.ApplicationDate,
                    a.ApplicationStatus
                })
                .ToList();

            var jobs = _context.Jobs
                .AsEnumerable()
                .Where(j => applications.Any(a => a.JobID == j.JobID))
                .Select(j =>
                {
                    var application = applications.FirstOrDefault(a => a.JobID == j.JobID);
                    return new UserApplicationsViewModel
                    {
                        AppId = application.ApplicationID,
                        JobId = j.JobID,
                        Title = j.Title,
                        JobType = JobCategoryTranslator.TranslateJobType(j.JobType),
                        Price = j.Price,
                        Country = JobCategoryTranslator.TranslateCountry(j.Country),
                        Description = j.Description,
                        ApplicationDate = application.ApplicationDate,
                        ApplicationStatus = application.ApplicationStatus,
                        CompletionPercentage = j.CompletionPercentage,
                        JobStatus = j.Status,
                        DeliveryDate = j.DeliveryDate,
                        JobCompleted = j.JobCompleted
                    };
                })
                .ToList();

            var notifications = _context.Notifications.Where(n => n.UserId.ToString() == userId && !n.IsRead).ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _context.Notifications.Update(notification);
            }

            _context.SaveChanges();

            return View(jobs);
        }

        // İşe başvuranları görüntüleme (GET)
        [HttpGet]
        public IActionResult JobApplications(int id, int page = 1, int pageSize = 10)
        {
            var jobApplications = _context.Applications
                .Where(j => j.JobID == id && j.ApplicationStatus == "Pending")
                .ToList();

            if (!jobApplications.Any())
            {
                var emptyList = new List<GetAllTranslatorViewModel>();
                var pagedEmptyList = emptyList.ToPagedList(page, pageSize);
                return View(pagedEmptyList);
            }

            // Başvuran kullanıcı bilgilerini al
            var users = _context.Users
                .AsEnumerable()  // Belleğe çekildikten sonra işle
                .Where(u => jobApplications.Any(j => j.UserID == u.Id))
                .Select(u => new GetAllTranslatorViewModel
                {
                    JobID = id,
                    UserID = u.Id,
                    PhotoUrl = u.PhotoUrl,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Occupation = u.Occupation,
                    Salary = u.Salary,
                    Experience = u.Experience,
                    Skills = u.Skills.ToList()
                })
                .ToList();

            var pagedUsers = users.ToPagedList(page, pageSize);
            return View(pagedUsers);
        }

        // İşveren tarafından çevirmenin onaylanması
        [HttpGet]
        public IActionResult ApproveApplication(int userId, int JobID)
        {
            var userid = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var application = _context.Applications.FirstOrDefault(a => a.UserID == userId && a.JobID == JobID);
            var job = _context.Jobs.FirstOrDefault(j => j.JobID == JobID);

            if (application == null)
            {
                return NotFound();
            }

            job.Status = "Active";
            application.ApplicationStatus = "Approved";

            var notification = new Notification
            {
                CreatedAt = DateTime.Now,
                IsRead = false,
                UserId = userId,
                Type = "Application",
            };
            _context.Notifications.Add(notification);

            var otherApplications = _context.Applications.Where(a => a.JobID == JobID && a.ApplicationID != application.ApplicationID).ToList();

            foreach (var otherApplication in otherApplications)
            {
                otherApplication.ApplicationStatus = "Rejected";

                var rejectionNotification = new Notification
                {
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    UserId = otherApplication.UserID,
                    Type = "Application",
                };
                _context.Notifications.Add(rejectionNotification);
            }

            _context.SaveChanges();
            return RedirectToAction("MyCreatedJobs", "Jobs");
        }

        // İşveren tarafından çevirmenin Reddedilmesi
        [HttpGet]
        public IActionResult RejectApplication(int userId, int JobID)
        {
            var userid = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var application = _context.Applications.FirstOrDefault(a => a.UserID == userId && a.JobID == JobID);
            var job = _context.Jobs.FirstOrDefault(j => j.JobID == JobID);

            if (application == null)
            {
                return NotFound();
            }

            job.Status = "Publish";
            application.ApplicationStatus = "Rejected";

            var notification = new Notification
            {
                CreatedAt = DateTime.Now,
                IsRead = false,
                UserId = userId,
                Type = "Application",
            };
            _context.Notifications.Add(notification);

            _context.SaveChanges();

            return RedirectToAction("MyCreatedJobs", "Jobs");
        }

    }
}
