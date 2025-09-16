using babiltr.EntityLayer;
using babiltr.Models.BLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text;
using X.PagedList.Extensions;


namespace babiltr.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tüm Blogların Listelenmesi
        [AllowAnonymous]
        public IActionResult AllBlogs(int page = 1, int pageSize = 12)
        {
            var blogs = _context.Blogs.ToList();
            var pagedBlogs = blogs.ToPagedList(page, pageSize);
            return View(pagedBlogs);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult BlogAdd()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult BlogAdd(Blog model, IFormFile blogImage)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;

                var userid = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (userid != null)
                {
                    var currentUser = _context.Users.FirstOrDefault(u => u.Id.ToString() == userid);
                    if (currentUser != null)
                    {
                        model.Author = $"{currentUser.FirstName} {currentUser.LastName}";
                    }
                }
                _context.Blogs.Add(model);
                _context.SaveChanges();

                // Dosya yükleme işlemi
                if (blogImage != null && blogImage.Length > 0)
                {
                    string fileExtension = Path.GetExtension(blogImage.FileName);
                    string fileName = $"{model.Title.Replace(" ", "_")}_{model.Id}{fileExtension}";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "BlogPictures", fileName);

                    using (var image = Image.Load(blogImage.OpenReadStream()))
                    {
                        image.Mutate(x => x.Resize(760, 440));
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image.Save(stream, new JpegEncoder());
                        }
                    }

                    model.ImageName = fileName;
                    _context.Blogs.Update(model);
                    _context.SaveChanges();
                }

                var parsedContents = ParseContent(model.Content);
                foreach (var content in parsedContents)
                {
                    var blogContent = new BlogContent
                    {
                        BlogId = model.Id,
                        Subheading = content.Subheading,
                        Paragraph = content.Paragraph
                    };
                    _context.BlogContents.Add(blogContent);
                }
                _context.SaveChanges();

                return RedirectToAction("AllBlogs");
            }

            return View(model);
        }

        public List<BlogContent> ParseContent(string content)
        {
            var parsedContents = new List<BlogContent>();
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            string currentSubheading = null;
            StringBuilder currentParagraph = new StringBuilder();

            foreach (var line in lines)
            {
                // Satırın başlık olup olmadığını kontrol ediyoruz
                if (line.Length <= 150 && char.IsLetterOrDigit(line.Trim()[0]))
                {
                    // Önceki başlık ve paragrafı kaydediyoruz
                    if (currentParagraph.Length > 0)
                    {
                        parsedContents.Add(new BlogContent
                        {
                            Subheading = currentSubheading,
                            Paragraph = currentParagraph.ToString().Trim()
                        });
                        currentParagraph.Clear();
                    }

                    // Yeni başlığı belirliyoruz
                    currentSubheading = line.Trim();
                }
                else
                {
                    // Paragrafa satır ekliyoruz
                    currentParagraph.AppendLine(line.Trim());
                }
            }

            // Son başlık ve paragrafı kaydediyoruz
            if (currentParagraph.Length > 0 || currentSubheading != null)
            {
                parsedContents.Add(new BlogContent
                {
                    Subheading = currentSubheading,
                    Paragraph = currentParagraph.ToString().Trim()
                });
            }

            return parsedContents;
        }

        // Blog Silme
        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult BlogDelete(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                _context.SaveChanges();
            }
            return RedirectToAction("AllBlogs");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult BlogDetails(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);
            if (blog == null)
            {
                return NotFound();
            }
            var blogContents = _context.BlogContents.Where(bc => bc.BlogId == id).ToList();
            ViewBag.Contents = blogContents;

            var latestBlogs = _context.Blogs
                .Where(b => b.Id != id)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToList();

            var tagCounts = _context.Blogs
                .Where(b => !string.IsNullOrEmpty(b.Tags))
                .AsEnumerable()
                .SelectMany(b => b.Tags.Split(','))
                .GroupBy(tag => tag.Trim())
                .Select(group => new { Tag = group.Key, Count = group.Count() })
                .OrderByDescending(tc => tc.Count)
                .Take(10)
                .ToList();

            var model = new BlogDetailViewModel
            {
                Blog = blog,
                LatestBlogs = latestBlogs,
                Tags = tagCounts.Select(tc => tc.Tag).ToList()
            };

            return View(model);
        }
    }
}
