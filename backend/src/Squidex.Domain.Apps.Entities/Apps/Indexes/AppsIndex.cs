// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes;

public sealed class AppsIndex : IAppsIndex, ICommandMiddleware, IInitializable
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly IAppRepository appRepository;
    private readonly IReplicatedCache appCache;
    private readonly NameReservationState namesState;

    public AppsIndex(IAppRepository appRepository, IReplicatedCache appCache,
        IPersistenceFactory<NameReservationState.State> persistenceFactory)
    {
        this.appRepository = appRepository;
        this.appCache = appCache;

        namesState = new NameReservationState(persistenceFactory, "Apps");
    }

    public Task InitializeAsync(
        CancellationToken ct)
    {
        return namesState.LoadAsync(ct);
    }

    public Task RemoveReservationAsync(string? token,
        CancellationToken ct = default)
    {
        return namesState.RemoveReservationAsync(token, ct);
    }

    public async Task<string?> ReserveAsync(DomainId id, string name,
        CancellationToken ct = default)
    {
        if (await appRepository.FindAsync(name, ct) != null)
        {
            return null;
        }

        return await namesState.ReserveAsync(id, name, ct);
    }

    public async Task<List<IAppEntity>> GetAppsForUserAsync(string userId, PermissionSet permissions,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("AppsIndex/GetAppsForUserAsync"))
        {
            var apps = await appRepository.QueryAllAsync(userId, permissions.ToAppNames(), ct);

            foreach (var app in apps.Where(IsValid))
            {
                await CacheItAsync(app);
            }

            return apps.Where(IsValid).ToList();
        }
    }

    public async Task<List<IAppEntity>> GetAppsForTeamAsync(DomainId teamId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("AppsIndex/GetAppsForTeamAsync"))
        {
            var apps = await appRepository.QueryAllAsync(teamId, ct);

            foreach (var app in apps.Where(IsValid))
            {
                await CacheItAsync(app);
            }

            return apps.Where(IsValid).ToList();
        }
    }

    public async Task<IAppEntity?> GetAppAsync(string name, bool canCache = false,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("AppsIndex/GetAppByNameAsync"))
        {
            if (canCache)
            {
                if (appCache.TryGetValue(GetCacheKey(name), out var v) && v is IAppEntity cacheApp)
                {
                    return cacheApp;
                }
            }

            var app = await appRepository.FindAsync(name, ct);

            if (!IsValid(app))
            {
                app = null;
            }

            if (app != null)
            {
                await CacheItAsync(app);
            }

            return app;
        }
    }

    public async Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache = false,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("AppsIndex/GetAppAsync"))
        {
            if (canCache)
            {
                if (appCache.TryGetValue(GetCacheKey(appId), out var cached) && cached is IAppEntity cachedApp)
                {
                    return cachedApp;
                }
            }

            var app = await appRepository.FindAsync(appId, ct);

            if (!IsValid(app))
            {
                app = null;
            }

            if (app != null)
            {
                await CacheItAsync(app);
            }

            return app;
        }
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        var command = context.Command;

        if (command is CreateApp createApp)
        {
            var token = await CheckAppAsync(createApp, ct);
            try
            {
                await next(context, ct);
            }
            finally
            {
                // Always remove the reservation and therefore do not pass over cancellation token.
                await namesState.RemoveReservationAsync(token, default);
            }
        }
        else
        {
            await next(context, ct);
        }

        if (context.IsCompleted)
        {
            switch (command)
            {
                case CreateApp create:
                    await OnCreateAsync(create);
                    break;
                case DeleteApp delete:
                    await OnDeleteAsync(delete);
                    break;
                case AppCommand update:
                    await OnUpdateAsync(update);
                    break;
            }
        }
    }

    private async Task<string?> CheckAppAsync(CreateApp command,
        CancellationToken ct)
    {
        var token = await ReserveAsync(command.AppId, command.Name, ct);

        if (token == null)
        {
            throw new ValidationException(T.Get("apps.nameAlreadyExists"));
        }

        return token;
    }

    private async Task OnCreateAsync(CreateApp create)
    {
        await InvalidateItAsync(create.AppId, create.Name);
    }

    private async Task OnDeleteAsync(DeleteApp delete)
    {
        await InvalidateItAsync(delete.AppId.Id, delete.AppId.Name);
    }

    private async Task OnUpdateAsync(AppCommand update)
    {
        await InvalidateItAsync(update.AppId.Id, update.AppId.Name);
    }

    private static string GetCacheKey(DomainId id)
    {
        return $"{typeof(AppsIndex)}_Apps_Id_{id}";
    }

    private static string GetCacheKey(string name)
    {
        return $"{typeof(AppsIndex)}_Apps_Name_{name}";
    }

    private static bool IsValid(IAppEntity? app)
    {
        return app is { Version: > EtagVersion.Empty, IsDeleted: false };
    }

    private Task InvalidateItAsync(DomainId id, string name)
    {
        return appCache.RemoveAsync(
            GetCacheKey(id),
            GetCacheKey(name));
    }

    private Task CacheItAsync(IAppEntity app)
    {
        return Task.WhenAll(
            appCache.AddAsync(GetCacheKey(app.Id), app, CacheDuration),
            appCache.AddAsync(GetCacheKey(app.Name), app, CacheDuration));
    }
}
