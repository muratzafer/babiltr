using babiltr.EntityLayer;
using babiltr.Models.USER;
using babiltr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace babiltr.Controllers
{
    public class UserController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment = null, UserManager<User> userManager = null)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        // Tüm Çevirmenleri görüntüleme
        public IActionResult GetAllTranslatorUser(int page = 1, int pageSize = 10)
        {
            var loggedInUserId = _userManager.GetUserId(User);
            var users = _context.Users
                .Include(u => u.Skills)
                .Where(t => t.Translator == true && t.Id.ToString() != loggedInUserId)
                .Select(u => new GetAllTranslatorViewModel
                {
                    UserID = u.Id,
                    PhotoUrl = u.PhotoUrl,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Occupation = u.Occupation,
                    Salary = u.Salary,
                    Experience = u.Experience,
                    Skills = u.Skills.ToList(),
                    ChatId = _context.Chats
                        .Where(c =>
                            (c.User1Id.ToString() == loggedInUserId && c.User2Id == u.Id) ||
                            (c.User2Id.ToString() == loggedInUserId && c.User1Id == u.Id))
                        .Select(c => c.Id)
                        .FirstOrDefault()
                })
                .ToList();

            var pagedUsers = users.ToPagedList(page, pageSize);
            return View(pagedUsers);
        }

        [HttpGet]
        // Tek bir kullanıcıyı görüntüleme
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.Include(u => u.Skills).Include(u => u.Educations).FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }
            var model = new GetUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DOB = user.DOB,
                Occupation = user.Occupation,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                City = user.City,
                Country = user.Country,
                CvOrResumeUrl = user.CvOrResumeUrl,
                PhotoUrl = user.PhotoUrl,
                Introduction = user.Introduction,
                Salary = user.Salary,
                Skills = user.Skills.ToList(),
                Educations = user.Educations.ToList(),
                Experience = user.Experience,
                Linkedin = user.Linkedin,
                Facebook = user.Facebook,
                Instagram = user.Instagram,
                Twitter = user.Twitter
            };

            return View(model);
        }

        [CheckAccess("EditUser")]
        // Kullanıcıyı güncelleme (get)
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Include(u => u.Skills).Include(u => u.Educations).FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }
            var model = new GetUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                DOB = user.DOB,
                Occupation = user.Occupation,
                Translator = user.Translator,
                City = user.City,
                Country = user.Country,
                PhotoUrl = user.PhotoUrl,
                CvOrResumeUrl = user.CvOrResumeUrl,
                Introduction = user.Introduction,
                Salary = user.Salary,
                Experience = user.Experience,
                Linkedin = user.Linkedin,
                Facebook = user.Facebook,
                Instagram = user.Instagram,
                Twitter = user.Twitter,
                Email = user.Email,
                Iban = user.Iban,
                PhoneNumber = user.PhoneNumber,

            };

            return View(model);
        }

        [HttpPost]
        // Profil fotoğraflarını güncelleme
        public async Task<IActionResult> UploadProfileImage(int id, IFormFile profileImage)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == id);

            if (profileImage != null && existingUser != null)
            {
                string firstName = existingUser.FirstName;
                string lastName = existingUser.LastName;
                int userId = existingUser.Id;

                string fileExtension = Path.GetExtension(profileImage.FileName);
                string newFileName = $"{firstName}_{lastName}_{userId}_pp{fileExtension}";

                var profileImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/PP", newFileName);

                using (var stream = new FileStream(profileImagePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                var imageUrl = $"/Images/PP/{newFileName}";
                existingUser.PhotoUrl = imageUrl;
                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                return Json(new { success = true, imageUrl });
            }

            return Json(new { success = false, message = "Resim yüklenemedi." });
        }

        [HttpPost]
        public IActionResult EditUser(GetUserViewModel user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);

                if (existingUser != null)
                {
                    // Zorunlu Alanlar
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.DOB = user.DOB;

                    // İş Bilgileri
                    existingUser.Translator = user.Translator;
                    existingUser.Occupation = user.Occupation;
                    existingUser.Salary = user.Salary;
                    existingUser.Experience = user.Experience;

                    // Optional Alanlar
                    if (!string.IsNullOrEmpty(user.Iban))
                        existingUser.Iban = user.Iban;
                    if (!string.IsNullOrEmpty(user.City))
                        existingUser.City = user.City;
                    if (!string.IsNullOrEmpty(user.Country))
                        existingUser.Country = user.Country;
                    if (!string.IsNullOrEmpty(user.Introduction))
                        existingUser.Introduction = user.Introduction;
                    if (!string.IsNullOrEmpty(user.Linkedin))
                        existingUser.Linkedin = user.Linkedin;
                    if (!string.IsNullOrEmpty(user.Facebook))
                        existingUser.Facebook = user.Facebook;
                    if (!string.IsNullOrEmpty(user.Instagram))
                        existingUser.Instagram = user.Instagram;
                    if (!string.IsNullOrEmpty(user.Twitter))
                        existingUser.Twitter = user.Twitter;

                    // CV dosyasını yükleme işlemi
                    if (user.CvOrResumeFile != null && user.CvOrResumeFile.Length > 0)
                    {
                        string firstName = existingUser.FirstName;
                        string lastName = existingUser.LastName;
                        int userId = existingUser.Id;

                        string fileExtension = Path.GetExtension(user.CvOrResumeFile.FileName);
                        string uniqueFileName = $"{firstName}_{lastName}_{userId}_cv{fileExtension}";

                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "CV");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            user.CvOrResumeFile.CopyTo(fileStream);
                        }

                        existingUser.CvOrResumeUrl = uniqueFileName;
                    }

                    // Kullanıcıyı güncelle
                    _context.Users.Update(existingUser);
                    _context.SaveChanges();

                    return RedirectToAction(nameof(GetUser), new { id = user.Id });
                }
            }
            return View(user);
        }

        // Eğitim Bilgisi Ekleme (POST)
        [HttpPost]
        public IActionResult AddEducation(AddEducationModel model)
        {
            if (ModelState.IsValid)
            {
                var newEducation = new Education
                {
                    UserID = model.UserID,
                    SchoolName = model.SchoolName,
                    Degree = model.Degree,
                    Years = model.Years
                };

                _context.Educations.Add(newEducation);
                _context.SaveChanges();
                return RedirectToAction(nameof(GetUser), new { id = model.UserID });

            }

            return View(model);
        }

        // Eğitim Bilgisi Silme (POST)
        [HttpGet]
        public IActionResult DeleteEducation(int id)
        {
            var education = _context.Educations.FirstOrDefault(e => e.EducationID == id);

            if (education != null)
            {
                _context.Educations.Remove(education);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(GetUser), new { id = education.UserID });
        }

        // Beceri Bilgisi Ekleme (POST)
        [HttpPost]
        public IActionResult AddSkills(AddSkillsModel model)
        {
            if (ModelState.IsValid)
            {
                var newSkills = new Skills
                {
                    UserID = model.UserID,
                    SkillName = model.SkillName,
                    SkillPercentage = model.SkillPercentage,
                };

                _context.Skills.Add(newSkills);

                _context.SaveChanges();
                return RedirectToAction(nameof(GetUser), new { id = model.UserID });

            }

            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteSkill(int id)
        {
            var skill = _context.Skills.FirstOrDefault(s => s.SkillsID == id);

            if (skill != null)
            {
                _context.Skills.Remove(skill);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(GetUser), new { id = skill.UserID });
        }
    }
}
