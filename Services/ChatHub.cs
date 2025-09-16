using Microsoft.AspNetCore.SignalR;

namespace babiltr.Services
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string senderId, string chatId, string content, string photoUrl, string userName)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            var turkeyTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var SentAt = turkeyTime.ToString("dd.MM.yyyy HH:mm");
            await Clients.Group(chatId).SendAsync("ReceiveMessage", senderId, userName, photoUrl, content, SentAt);
        }
        public override async Task OnConnectedAsync()
        {
            var chatId = Context.GetHttpContext().Request.Query["chatId"];
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            await base.OnConnectedAsync();
        }
    }
}
