﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Notifications;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Email;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public sealed class UsageNotifierGrain : GrainOfString, IUsageNotifierGrain
    {
        private static readonly TimeSpan TimeBetweenNotifications = TimeSpan.FromHours(12);
        private readonly IGrainState<State> state;
        private readonly INotificationSender notificationSender;
        private readonly IUserResolver userResolver;

        [CollectionName("UsageNotifications")]
        public sealed class State
        {
            public Dictionary<Guid, DateTime> NotificationsSent { get; } = new Dictionary<Guid, DateTime>();
        }

        public UsageNotifierGrain(IGrainState<State> state, INotificationSender notificationSender, IUserResolver userResolver)
        {
            Guard.NotNull(state);
            Guard.NotNull(notificationSender);
            Guard.NotNull(userResolver);

            this.state = state;
            this.notificationSender = notificationSender;
            this.userResolver = userResolver;
        }

        public async Task NotifyAsync(UsageNotification notification)
        {
            var now = DateTime.UtcNow;

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

        private bool HasBeenSentBefore(Guid appId, DateTime now)
        {
            if (state.Value.NotificationsSent.TryGetValue(appId, out var lastSent))
            {
                var elapsed = now - lastSent;

                return elapsed > TimeBetweenNotifications;
            }

            return false;
        }

        private Task TrackNotifiedAsync(Guid appId, DateTime now)
        {
            state.Value.NotificationsSent[appId] = now;

            return state.WriteAsync();
        }
    }
}
