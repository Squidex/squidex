// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.Notifications;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Messaging;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public sealed class UsageNotifierWorker : IMessageHandler<UsageTrackingCheck>
    {
        private static readonly TimeSpan TimeBetweenNotifications = TimeSpan.FromDays(3);
        private readonly SimpleState<State> state;
        private readonly INotificationSender notificationSender;
        private readonly IUserResolver userResolver;

        [CollectionName("UsageNotifications")]
        public sealed class State
        {
            public Dictionary<DomainId, DateTime> NotificationsSent { get; } = new Dictionary<DomainId, DateTime>();
        }

        public IClock Clock { get; set; } = SystemClock.Instance;

        public UsageNotifierWorker(IPersistenceFactory<State> persistenceFactory,
            INotificationSender notificationSender, IUserResolver userResolver)
        {
            this.notificationSender = notificationSender;
            this.userResolver = userResolver;

            state = new SimpleState<State>(persistenceFactory, GetType(), DomainId.Create("Default"));
        }

        public async Task HandleAsync(UsageTrackingCheck notification,
            CancellationToken ct)
        {
            if (!notificationSender.IsActive)
            {
                return;
            }

            var now = Clock.GetCurrentInstant().ToDateTimeUtc();

            if (!HasBeenSentBefore(notification.AppId, now))
            {
                if (notificationSender.IsActive)
                {
                    foreach (var userId in notification.Users)
                    {
                        var user = await userResolver.FindByIdOrEmailAsync(userId, ct);

                        if (user != null)
                        {
                            notificationSender.SendUsageAsync(user,
                                notification.AppName,
                                notification.Usage,
                                notification.UsageLimit).Forget();
                        }
                    }
                }

                await TrackNotifiedAsync(notification.AppId, now);
            }
        }

        private bool HasBeenSentBefore(DomainId appId, DateTime now)
        {
            if (state.Value.NotificationsSent.TryGetValue(appId, out var lastSent))
            {
                var elapsed = now - lastSent;

                return elapsed < TimeBetweenNotifications;
            }

            return false;
        }

        private Task TrackNotifiedAsync(DomainId appId, DateTime now)
        {
            state.Value.NotificationsSent[appId] = now;

            return state.WriteAsync();
        }
    }
}
