using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace babiltr.Services
{
    public class CheckAccessAttribute : ActionFilterAttribute
    {
        private readonly string _entity;

        public CheckAccessAttribute(string entity)
        {
            _entity = entity;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            var currentUserId = httpContext.Request.Cookies["UserId"];

            bool hasAccess = false;

            var jobId = context.ActionArguments.ContainsKey("id")
                        ? (int?)context.ActionArguments["id"]
                        : null;

            var dbContext = (ApplicationDbContext)context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext));

            switch (_entity)
            {
                case "ShowSegments":
                    if (jobId.HasValue)
                    {
                        var job = dbContext.Jobs.FirstOrDefault(j => j.JobID == jobId.Value);
                        if (job != null && job.UserID.ToString() == currentUserId)
                        {
                            hasAccess = true;
                        }
                    }
                    break;

                case "Translation":
                    if (jobId.HasValue)
                    {
                        var job = dbContext.Jobs.FirstOrDefault(j => j.JobID == jobId.Value);
                        if (job != null && (job.Status == "Active" || job.Status == "Revision"))
                        {
                            var applications = dbContext.Applications.FirstOrDefault(a => a.JobID == job.JobID && a.UserID.ToString() == currentUserId && a.ApplicationStatus == "Approved");

                            if (applications != null)
                            {
                                hasAccess = true;
                            }
                        }
                    }
                    break;

                case "EditUser":
                    var edituserId = context.ActionArguments.ContainsKey("id")
                                     ? context.ActionArguments["id"].ToString()
                                     : null;
                    if (edituserId != null && edituserId == currentUserId)
                    {
                        hasAccess = true;
                    }
                    break;

                case "Chat":
                    var chatId = context.ActionArguments.ContainsKey("chatid")
                                 ? context.ActionArguments["chatid"].ToString()
                                 : null;
                    var userId = context.ActionArguments.ContainsKey("userid")
                                 ? context.ActionArguments["userid"].ToString()
                                 : null;

                    // Eğer chatId varsa mevcut sohbeti kontrol et, chatId yoksa ve userId varsa yeni sohbet başlatılmasına izin ver
                    if (!string.IsNullOrEmpty(chatId))
                    {
                        var chat = dbContext.Chats.FirstOrDefault(c => c.Id == int.Parse(chatId));
                        if (chat != null)
                        {
                            if ((chat.User1Id.ToString() == currentUserId || chat.User2Id.ToString() == currentUserId))
                            {
                                hasAccess = true;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(userId))
                    {
                        hasAccess = true;
                    }
                    break;


                default:
                    hasAccess = false;
                    break;
            }

            if (!hasAccess)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }

            base.OnActionExecuting(context);
        }
    }
}
