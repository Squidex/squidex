// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppPermanentDeleter(
    IEnumerable<IDeleter> deleters,
    IOptions<AppsOptions> options,
    IDomainObjectFactory factory,
    TypeRegistry typeRegistry)
    : IEventConsumer
{
    private readonly IEnumerable<IDeleter> deleters = deleters.OrderBy(x => x.Order).ToList();
    private readonly AppsOptions options = options.Value;
    private readonly HashSet<string> consumingTypes =
        [
            typeRegistry.GetName<IEvent, AppDeleted>(),
            typeRegistry.GetName<IEvent, AppContributorRemoved>(),
        ];

    public StreamFilter EventsFilter { get; } = StreamFilter.Prefix("app-");

    public ValueTask<bool> HandlesAsync(StoredEvent @event)
    {
        return new ValueTask<bool>(consumingTypes.Contains(@event.Data.Type));
    }

    public async Task On(Envelope<IEvent> @event)
    {
        if (@event.Headers.Restored())
        {
            return;
        }

        switch (@event.Payload)
        {
            case AppDeleted appDeleted:
                await OnDeleteAsync(appDeleted);
                break;
            case AppContributorRemoved appContributorRemoved:
                await OnAppContributorRemoved(appContributorRemoved);
                break;
        }
    }

    private async Task OnAppContributorRemoved(AppContributorRemoved appContributorRemoved)
    {
        using var activity = Telemetry.Activities.StartActivity("RemoveContributorFromSystem");

        var appId = appContributorRemoved.AppId.Id;

        foreach (var deleter in deleters)
        {
            using (Telemetry.Activities.StartActivity(deleter.GetType().Name))
            {
                await deleter.DeleteContributorAsync(appId, appContributorRemoved.ContributorId, default);
            }
        }
    }

    private async Task OnDeleteAsync(AppDeleted appDeleted)
    {
        // The user can either remove the app itself or via a global setting for all apps.
        if (!appDeleted.Permanent && !options.DeletePermanent)
        {
            return;
        }

        using var activity = Telemetry.Activities.StartActivity("RemoveAppFromSystem");

        var app = await GetAppAsync(appDeleted.AppId.Id);
        if (app == null)
        {
            return;
        }

        foreach (var deleter in deleters)
        {
            using (Telemetry.Activities.StartActivity(deleter.GetType().Name))
            {
                await deleter.DeleteAppAsync(app.Snapshot, default);
            }
        }
    }

    private async Task<AppDomainObject?> GetAppAsync(DomainId appId)
    {
        // Bypass our normal resolve process, so that we can also retrieve the deleted app.
        var app = factory.Create<AppDomainObject>(appId);

        await app.EnsureLoadedAsync();

        // If the app does not exist, the version is lower than zero.
        return app.Version < 0 ? null : app;
    }
}
