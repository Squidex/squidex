// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MassTransit;
using NodaTime;
using Squidex.Domain.Apps.Entities.Notifications;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public sealed class UsageNotifierWorker : IConsumer<UsageTrackingCheck>
    {
        private static readonly TimeSpan TimeBetweenNotifications = TimeSpan.FromDays(3);
        private readonly SimpleState<State> state;
        private readonly INotificationSender notificationSender;
        private readonly IUserResolver userResolver;
        private readonly IClock clock;

        [CollectionName("UsageNotifications")]
        public sealed class State
        {
            public Dictionary<DomainId, DateTime> NotificationsSent { get; } = new Dictionary<DomainId, DateTime>();
        }

        public UsageNotifierWorker(IPersistenceFactory<State> persistenceFactory,
            INotificationSender notificationSender, IUserResolver userResolver, IClock clock)
        {
            this.notificationSender = notificationSender;
            this.userResolver = userResolver;
            this.clock = clock;

            state = new SimpleState<State>(persistenceFactory, GetType(), DomainId.Create("Default"));
        }

        public Task Consume(ConsumeContext<UsageTrackingCheck> context)
        {
            return NotifyAsync(context.Message);
        }

        public async Task NotifyAsync(UsageTrackingCheck notification)
        {
            if (!notificationSender.IsActive)
            {
                return;
            }

            var now = clock.GetCurrentInstant().ToDateTimeUtc();

            if (!HasBeenSentBefore(notification.AppId, now))
            {
                if (notificationSender.IsActive)
                {
                    foreach (var userId in notification.Users)
                    {
                        var user = await userResolver.FindByIdOrEmailAsync(userId);

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
