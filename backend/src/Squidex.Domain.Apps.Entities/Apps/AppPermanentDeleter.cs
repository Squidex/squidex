// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppPermanentDeleter : IEventConsumer
{
    private readonly IEnumerable<IDeleter> deleters;
    private readonly IDomainObjectFactory factory;
    private readonly HashSet<string> consumingTypes;

    public StreamFilter EventsFilter { get; } = StreamFilter.Prefix("app-");

    public AppPermanentDeleter(IEnumerable<IDeleter> deleters, IDomainObjectFactory factory, TypeRegistry typeRegistry)
    {
        this.deleters = deleters.OrderBy(x => x.Order).ToList();
        this.factory = factory;

        // Compute the event types names once for performance reasons and use hashset for extensibility.
        consumingTypes =
        [
            typeRegistry.GetName<IEvent, AppDeleted>(),
            typeRegistry.GetName<IEvent, AppContributorRemoved>()
        ];
    }

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
            case AppDeleted appArchived:
                await OnArchiveAsync(appArchived);
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

    private async Task OnArchiveAsync(AppDeleted appArchived)
    {
        using var activity = Telemetry.Activities.StartActivity("RemoveAppFromSystem");

        // Bypass our normal app resolve process, so that we can also retrieve the deleted app.
        var app = factory.Create<AppDomainObject>(appArchived.AppId.Id);

        await app.EnsureLoadedAsync();

        // If the app does not exist, the version is lower than zero.
        if (app.Version < 0)
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
}
