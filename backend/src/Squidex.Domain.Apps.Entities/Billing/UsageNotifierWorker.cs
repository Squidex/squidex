// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.Notifications;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Messaging;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Billing;

public sealed class UsageNotifierWorker : IMessageHandler<UsageTrackingCheck>, IInitializable
{
    private static readonly TimeSpan TimeBetweenNotifications = TimeSpan.FromDays(3);
    private readonly SimpleState<State> state;
    private readonly IAppProvider appProvider;
    private readonly IUserNotifications userNotifications;
    private readonly IUserResolver userResolver;

    [CollectionName("UsageNotifications")]
    public sealed class State
    {
        public Dictionary<DomainId, DateTime> NotificationsSent { get; set; } = new Dictionary<DomainId, DateTime>();
    }

    public IClock Clock { get; set; } = SystemClock.Instance;

    public UsageNotifierWorker(IPersistenceFactory<State> persistenceFactory,
        IAppProvider appProvider,
        IUserNotifications userNotifications,
        IUserResolver userResolver)
    {
        this.appProvider = appProvider;
        this.userNotifications = userNotifications;
        this.userResolver = userResolver;

        state = new SimpleState<State>(persistenceFactory, GetType(), DomainId.Create("Default"));
    }

    public Task InitializeAsync(
        CancellationToken ct)
    {
        return state.LoadAsync(ct);
    }

    public async Task HandleAsync(UsageTrackingCheck notification,
        CancellationToken ct)
    {
        if (!userNotifications.IsActive)
        {
            return;
        }

        var now = Clock.GetCurrentInstant().ToDateTimeUtc();

        if (!HasBeenSentBefore(notification.AppId, now))
        {
            await SendAsync(notification, ct);

            await TrackNotifiedAsync(notification.AppId, now);
        }
    }

    private async Task SendAsync(UsageTrackingCheck notification,
        CancellationToken ct)
    {
        if (!userNotifications.IsActive)
        {
            return;
        }

        var app = await appProvider.GetAppAsync(notification.AppId, true, ct);

        if (app == null)
        {
            return;
        }

        foreach (var userId in notification.Users)
        {
            var user = await userResolver.FindByIdOrEmailAsync(userId, ct);

            if (user != null)
            {
                await userNotifications.SendUsageAsync(user, app,
                    notification.Usage,
                    notification.UsageLimit, ct);
            }
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
