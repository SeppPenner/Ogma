using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data.Enums;
using Ogma3.Data.Models;

namespace Ogma3.Data.Repositories
{
    public class NotificationsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IUrlHelper _urlHelper;

        public NotificationsRepository(ApplicationDbContext context, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _context = context;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        private static string Message(ENotificationEvent @event) => @event switch
        {
            ENotificationEvent.System => "[SYSTEM]",
            ENotificationEvent.WatchedStoryUpdated => "The story you're watching just updated.",
            ENotificationEvent.WatchedThreadNewComment => "The comments thread you're following has a new comment.",
            ENotificationEvent.FollowedAuthorNewBlogpost => "The author you're following just wrote a new blogpost.",
            ENotificationEvent.FollowedAuthorNewStory => "The author you're following just created a new story.",
            ENotificationEvent.CommentReply => "One of your comments just got a reply.",
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
        };

        public async Task Create(ENotificationEvent @event, IEnumerable<long> recipientIds, string page, object routeData, string? body = null)
        {
            var notification = new Notification
            {
                Body = body ?? Message(@event),
                Event = @event,
                Url = _urlHelper.Page(page, routeData)
            };
            await _context.Notifications.AddAsync(notification);

            var notificationRecipients = recipientIds
                .Select(u => new NotificationRecipients {RecipientId = u, Notification = notification});
            
            await _context.NotificationRecipients.AddRangeAsync(notificationRecipients);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetForUser(long user)
        {
            return await _context.NotificationRecipients
                .Where(nr => nr.RecipientId == user)
                .Select(nr => nr.Notification)
                .ToListAsync();
        }

        public async Task<int> CountForUser(long user)
        {
            return await _context.NotificationRecipients
                .Where(nr => nr.RecipientId == user)
                .CountAsync();
        }

        public async Task Delete(long id, long uid)
        {
            var notificationRecipient = await _context.NotificationRecipients
                .Where(nr => nr.RecipientId == uid)
                .Where(nr => nr.NotificationId == id)
                .FirstOrDefaultAsync();
            
            if (notificationRecipient is null) return;

            _context.NotificationRecipients.Remove(notificationRecipient);
            await _context.SaveChangesAsync();
        }
    }
}