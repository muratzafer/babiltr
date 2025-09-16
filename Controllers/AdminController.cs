using babiltr.EntityLayer;
using babiltr.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace babiltr.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager = null)
        {
            _context = context;
            _userManager = userManager;
        }
        [AllowAnonymous]
        [HttpGet("Admin/Ping")]
        public IActionResult Ping() => Content("admin ok");


        //Admin Dashboard Sayfası
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("user");
            var totalUsers = usersInRole.Count();
            var totalJobs = _context.Jobs.Count();
            var totalApplications = _context.Applications.Count();

            // Bugünkü kullanıcı, iş ve başvuru sayısını al
            var today = DateTime.Today;
            var todayUsers = usersInRole.Count(u => u.RegisterDate.Date == today);
            var todayJobs = _context.Jobs.Count(j => j.PostedDate.Date == today);
            var todayApplications = _context.Applications.Count(a => a.ApplicationDate.Date == today);

            // Önceki gün verilerini al
            var yesterday = today.AddDays(-1);
            var yesterdayUsers = usersInRole.Count(u => u.RegisterDate.Date == yesterday);
            var yesterdayJobs = _context.Jobs.Count(j => j.PostedDate.Date == yesterday);
            var yesterdayApplications = _context.Applications.Count(a => a.ApplicationDate.Date == yesterday);

            // Günlük artış oranlarını hesapla
            var userGrowth = CalculateGrowth(todayUsers, yesterdayUsers);
            var jobGrowth = CalculateGrowth(todayJobs, yesterdayJobs);
            var applicationGrowth = CalculateGrowth(todayApplications, yesterdayApplications);

            // Haftalık verileri almak için
            var weekUsers = new List<int>();
            var weekJobs = new List<int>();
            var weekApplications = new List<int>();
            var lastMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

            // Son 7 gün için verileri al
            for (int i = 0; i < 7; i++)
            {
                var date = lastMonday.AddDays(i);
                var dailyUsers = usersInRole.Count(u => u.RegisterDate.Date == date);
                var dailyJobs = _context.Jobs.Count(j => j.PostedDate.Date == date);
                var dailyApplications = _context.Applications.Count(a => a.ApplicationDate.Date == date);

                weekUsers.Add(dailyUsers);
                weekJobs.Add(dailyJobs);
                weekApplications.Add(dailyApplications);
            }

            var stats = new AdminStatsViewModel
            {
                TotalUsers = totalUsers,
                TotalJobs = totalJobs,
                TotalApplications = totalApplications,
                UserGrowth = userGrowth,
                JobGrowth = jobGrowth,
                ApplicationGrowth = applicationGrowth,
                WeeklyUsers = weekUsers,
                WeeklyJobs = weekJobs,
                WeeklyApplications = weekApplications
            };

            return View(stats);
        }

        // Fark hesaplama
        private int CalculateGrowth(int todayCount, int yesterdayCount)
        {
            if (yesterdayCount > 0)
            {
                return ((todayCount - yesterdayCount)) / yesterdayCount;
            }
            return 0;
        }

        // Admin Sayfası Tüm Kullanıcılar
        [HttpGet]
        public async Task<IActionResult> AllUsers(int page = 1, int pageSize = 10)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("admin");
            var users = _context.Users
                .Where(u => !usersInRole.Contains(u))
                .Select(u => new AdminGetAllUsersViewModel
                {
                    UserID = u.Id,
                    PhotoUrl = u.PhotoUrl,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Occupation = u.Occupation,
                    Translator = u.Translator,
                    RegisterDate = u.RegisterDate
                })
                .ToList();
            var pagedUsers = users.ToPagedList(page, pageSize);

            return View(pagedUsers);
        }

        // Admin Sayfası Tüm İşler
        [HttpGet]
        public IActionResult AllJobs(int page = 1, int pageSize = 10)
        {
            var jobs = _context.Jobs.ToList();
            var jobViewModels = jobs.Select(job => new AdminAllJobsViewModel
            {

                JobId = job.JobID,
                Title = job.Title,
                Price = job.Price,
                Status = job.Status,
                CompletionPercentage = job.CompletionPercentage,
                UserID = job.UserID,
                PostedDate = job.PostedDate,
                ExpiryDate = job.ExpiryDate
            }).ToList();

            var pagedJobs = jobViewModels.ToPagedList(page, pageSize);
            return View(pagedJobs);
        }

        // Admin Sayfası Tüm Başvurular
        [HttpGet]
        public async Task<IActionResult> AllApplications(int page = 1, int pageSize = 10)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("admin");
            var users = await _context.Users
                .Where(u => !usersInRole.Contains(u))
                .ToListAsync();

            var jobs = await _context.Jobs.ToListAsync();
            var applications = await _context.Applications.ToListAsync();

            var applicationViewModels = applications.Select(a => new AdminAllAppViewModel
            {
                AppId = a.ApplicationID,
                JobId = a.JobID,

                EmployerPhotoUrl = users.FirstOrDefault(u => u.Id == jobs.FirstOrDefault(j => j.JobID == a.JobID)?.UserID)?.PhotoUrl,
                EmployerName = users.FirstOrDefault(u => u.Id == jobs.FirstOrDefault(j => j.JobID == a.JobID)?.UserID)?.FirstName,
                EmployerLastName = users.FirstOrDefault(u => u.Id == jobs.FirstOrDefault(j => j.JobID == a.JobID)?.UserID)?.LastName,
                EmployerEmail = users.FirstOrDefault(u => u.Id == jobs.FirstOrDefault(j => j.JobID == a.JobID)?.UserID)?.Email,

                TranslatorPhotoUrl = users.FirstOrDefault(u => u.Id == a.UserID)?.PhotoUrl,
                TranslatorName = users.FirstOrDefault(u => u.Id == a.UserID)?.FirstName,
                TranslatorLastName = users.FirstOrDefault(u => u.Id == a.UserID)?.LastName,
                TranslatorEmail = users.FirstOrDefault(u => u.Id == a.UserID)?.Email,

                // Job title and application details
                Title = jobs.FirstOrDefault(j => j.JobID == a.JobID)?.Title,
                ApplicationStatus = a.ApplicationStatus,
                ApplicationDate = a.ApplicationDate
            }).ToList();

            var pagedApplications = applicationViewModels.ToPagedList(page, pageSize);
            return View(pagedApplications);
        }

        // Admin Sayfası Komisyon ayarlarını gösterme (get)
        [HttpGet]
        public async Task<IActionResult> Commission()
        {
            var commission = await _context.Commissions.FirstOrDefaultAsync();

            if (commission == null)
            {
                return View(new Commission { EmployerPercentage = 0 });
            }

            return View(commission);
        }

        // Admin Sayfası İşveren Komisyon ayarlarını değiştirme (post)
        [HttpPost]
        public async Task<IActionResult> EmployerCommission(Commission commission)
        {
            if (ModelState.IsValid)
            {
                var existingCommission = await _context.Commissions.FirstOrDefaultAsync();

                if (commission.EmployerPercentage > 100)
                {
                    commission.EmployerPercentage /= 100;
                }

                if (existingCommission != null)
                {
                    existingCommission.EmployerPercentage = commission.EmployerPercentage;
                    TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                    existingCommission.LastUpdated = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
                }
                else
                {
                    TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

                    _context.Commissions.Add(new Commission
                    {
                        EmployerPercentage = commission.EmployerPercentage,
                        LastUpdated = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone),
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Commission));
            }

            return RedirectToAction(nameof(Commission));
        }

        // Admin Sayfası Çevirmen Komisyon ayarlarını değiştirme (post)
        [HttpPost]
        public async Task<IActionResult> TranslatorCommission(Commission commission)
        {
            if (ModelState.IsValid)
            {
                var existingCommission = await _context.Commissions.FirstOrDefaultAsync();

                if (commission.TranslatorPercentage > 100)
                {
                    commission.TranslatorPercentage /= 100;
                }

                if (existingCommission != null)
                {
                    existingCommission.TranslatorPercentage = commission.TranslatorPercentage;
                    TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                    existingCommission.LastUpdated = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
                }
                else
                {
                    TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

                    _context.Commissions.Add(new Commission
                    {
                        TranslatorPercentage = commission.EmployerPercentage,
                        LastUpdated = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone),
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Commission));
            }

            return RedirectToAction(nameof(Commission));
        }

        // Admin Sayfası Tüm Destek Talepleri
        [HttpGet]
        public async Task<IActionResult> AllMessages(int page = 1, int pageSize = 10)
        {
            var messages = await _context.Contacts
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var pagedMessages = messages.ToPagedList(page, pageSize);
            return View(pagedMessages);
        }

        // Admin Sayfası Destek Talebinin Silinme Talebi
        [HttpGet]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Contacts.FindAsync(id);
            _context.Contacts.Remove(message);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllMessages));
        }
    }
}
