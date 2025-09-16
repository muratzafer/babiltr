using Microsoft.AspNetCore.Mvc;

namespace babiltr.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Bildirim Kaydetme ve Sayma
        [HttpGet]
        public JsonResult GetUnreadCounts(int userID)
        {
            int applicationCount = _context.Notifications
                .Count(n => n.UserId == userID && n.Type == "Application" && !n.IsRead);
            int messageCount = _context.Notifications
                .Count(n => n.UserId == userID && n.Type == "Message" && !n.IsRead);

            var unreadCounts = new Dictionary<string, int>
            {
                { "applicationCount", applicationCount }
              , { "messageCount", messageCount }
            };

            return Json(unreadCounts);
        }
    }
}
