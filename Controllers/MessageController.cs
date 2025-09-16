using babiltr.EntityLayer;
using babiltr.Models.Message;
using babiltr.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace babiltr.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(ApplicationDbContext context, IHubContext<ChatHub> hubContext = null)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Kullanıcının Mesajlaştığı Sohbetleri getir
        [HttpGet]
        public async Task<IActionResult> ChatUsers(int page = 1, int pageSize = 10)
        {
            int userId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out var temp) ? temp : 0;

            // Kullanıcının mesajlaştığı kişileri al
            var chatUsers = await _context.Messages
                .Where(m => (m.Chat.User1Id == userId || m.Chat.User2Id == userId) && m.Chat.DeleteId != userId)
                .GroupBy(m => m.Chat.User1Id == userId ? m.Chat.User2Id : m.Chat.User1Id)
                .Select(g => new ChatUserViewModel
                {
                    UserId = userId,
                    ChatId = _context.Chats.Where(c => (c.User1Id == userId && c.User2Id == (int)g.Key) || (c.User2Id == userId && c.User1Id == (int)g.Key)).Select(c => c.Id).FirstOrDefault(),
                    FullName = _context.Users.FirstOrDefault(u => u.Id == g.Key).FirstName + " " + _context.Users.FirstOrDefault(u => u.Id == g.Key).LastName,
                    UserPhoto = _context.Users.FirstOrDefault(u => u.Id == g.Key).PhotoUrl,
                    LastMessageContent = g.OrderByDescending(m => m.SentAt).Select(m => m.Content).FirstOrDefault(),
                    LastMessageDate = g.OrderByDescending(m => m.SentAt).Select(m => m.SentAt).FirstOrDefault(),
                    LastMessageSenderId = (int)g.OrderByDescending(m => m.SentAt).Select(m => m.Chat.User1Id).FirstOrDefault()
                })
                .ToListAsync();

            // Sohbetlerden mesaj olmayanları sil
            var chatsToRemove = await _context.Chats
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Where(c => !_context.Messages.Any(m => m.ChatId == c.Id))
                .ToListAsync();

            _context.Chats.RemoveRange(chatsToRemove);
            await _context.SaveChangesAsync();

            var pagedChatUsers = chatUsers.ToPagedList(page, pageSize);
            return View(pagedChatUsers);
        }

        // Mesaj Sayfası ve varsa önceki mesajların getirildiği sayfa
        [HttpGet]
        [CheckAccess("Chat")]
        public async Task<IActionResult> Chat(int chatid, int userid)
        {
            int userId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out var temp) ? temp : 0;

            var chat = await _context.Chats.FindAsync(chatid);
            if (chat == null)
            {
                var User = _context.Users.FirstOrDefault(u => u.Id == userId);
                chat = new Chat
                {
                    User1Id = userId,
                    User2Id = userid
                };
                var OtherUser = _context.Users.FirstOrDefault(u => u.Id == userid);
                TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                chat.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                var chatViewModel = new ChatViewModel
                {
                    UserId = userId,
                    ChatId = chat.Id,
                    PhotoUrl = User.PhotoUrl,
                    UserName = $"{User?.FirstName} {User?.LastName}",
                    IsBlocked = chat.IsBlocked,
                    OtherUser = OtherUser,
                    Messages = null
                };
                return View(chatViewModel);
            }

            int receiverId = chat.User1Id == userId ? chat.User2Id : chat.User1Id;

            var messages = await _context.Messages
                .Where(m => m.ChatId == chatid)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var otherUser = _context.Users.FirstOrDefault(u => u.Id == receiverId);

            var chatViewModelWithMessages = new ChatViewModel
            {
                UserId = userId,
                ChatId = chat.Id,
                PhotoUrl = user.PhotoUrl,
                UserName = $"{user?.FirstName} {user?.LastName}",
                IsBlocked = chat.IsBlocked,
                OtherUser = otherUser,
                Messages = messages.Select(m =>
                {
                    var sender = _context.Users.FirstOrDefault(u => u.Id == m.SenderId);

                    return new MessageViewModel
                    {
                        UserId = userId,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        Content = m.Content,
                        SentAt = m.SentAt,
                        PhotoUrl = sender?.PhotoUrl,
                        UserName = $"{sender?.FirstName} {sender?.LastName}",
                    };
                }).ToList()
            };

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.CreatedAt <= DateTime.UtcNow && n.ChatId == chatid)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _context.Notifications.Update(notification);
            }
            await _context.SaveChangesAsync();
            return View(chatViewModelWithMessages);
        }

        // Gönderilen Mesajların veritabanına kaydedilmesi
        [HttpPost]
        public async Task<IActionResult> SendMessage(int chatId, string content)
        {
            int senderId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out var temp) ? temp : 0;

            var chat = await _context.Chats.FindAsync(chatId);
            if (chat == null)
            {
                return NotFound();
            }

            int receiverId;
            if (chat.User1Id == senderId)
            {
                receiverId = chat.User2Id;
            }
            else
            {
                receiverId = chat.User1Id;
            }

            var message = new EntityLayer.Message
            {
                ChatId = chatId,
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
            };

            TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            message.SentAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var notification = new EntityLayer.Notification
            {
                UserId = receiverId,
                Type = "Message",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                ChatId = chatId,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Sohbet Silme
        [HttpGet]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var chat = await _context.Chats.FindAsync(id);

            int userId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out var temp) ? temp : 0;

            if (chat.DeleteId != 0 && chat.DeleteId != userId)
            {
                var messagesToDelete = await _context.Messages
                    .Where(m => (m.SenderId == chat.User1Id && m.ReceiverId == chat.User2Id) ||
                                 (m.SenderId == chat.User2Id && m.ReceiverId == chat.User1Id)).ToListAsync();

                _context.Messages.RemoveRange(messagesToDelete);
                _context.Chats.Remove(chat);
            }
            else
            {
                chat.DeleteId = userId;
                _context.Chats.Update(chat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ChatUsers");
        }

        // Kişi Engelleme
        [HttpGet]
        public async Task<IActionResult> BlockChat(int id)
        {
            var chat = await _context.Chats.FindAsync(id);

            chat.IsBlocked = true;
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync();
            return RedirectToAction("ChatUsers");
        }
    }
}
