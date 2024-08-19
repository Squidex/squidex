// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Apps;
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
    private readonly IAppRepository appRepository;
    private readonly IReplicatedCache appCache;
    private readonly AppCacheOptions options;
    private readonly NameReservationState namesState;

    public AppsIndex(IAppRepository appRepository, IReplicatedCache appCache,
        IPersistenceFactory<NameReservationState.State> persistenceFactory,
        IOptions<AppCacheOptions> options)
    {
        this.appRepository = appRepository;
        this.appCache = appCache;
        this.options = options.Value;

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

    public async Task<List<App>> GetAppsForUserAsync(string userId, PermissionSet permissions,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("AppsIndex/GetAppsForUserAsync"))
        {
            activity?.SetTag("userId", userId);

            var apps = await appRepository.QueryAllAsync(userId, permissions.ToAppNames(), ct);

            return await apps.Where(IsValid).SelectAsync(PrepareAsync);
        }
    }

    public async Task<List<App>> GetAppsForTeamAsync(DomainId teamId,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("AppsIndex/GetAppsForTeamAsync"))
        {
            activity?.SetTag("teamId", teamId);

            var apps = await appRepository.QueryAllAsync(teamId, ct);

            return await apps.Where(IsValid).SelectAsync(PrepareAsync);
        }
    }

    public async Task<App?> GetAppAsync(string name, bool canCache = false,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("AppsIndex/GetAppByNameAsync"))
        {
            activity?.SetTag("appName", name);

            if (canCache)
            {
                if (appCache.TryGetValue(GetCacheKey(name), out var v) && v is App cacheApp)
                {
                    return cacheApp;
                }
            }

            var app = await appRepository.FindAsync(name, ct);

            if (app == null || !IsValid(app))
            {
                return null;
            }

            return await PrepareAsync(app);
        }
    }

    public async Task<App?> GetAppAsync(DomainId appId, bool canCache = false,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("AppsIndex/GetAppAsync"))
        {
            activity?.SetTag("appId", appId);

            if (canCache)
            {
                if (appCache.TryGetValue(GetCacheKey(appId), out var cached) && cached is App cachedApp)
                {
                    return cachedApp;
                }
            }

            var app = await appRepository.FindAsync(appId, ct);

            if (app == null || !IsValid(app))
            {
                return null;
            }

            return await PrepareAsync(app);
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
        var token = await ReserveAsync(command.AppId, command.Name, ct)
            ?? throw new ValidationException(T.Get("apps.nameAlreadyExists"));

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

    private static bool IsValid(App? app)
    {
        return app is { Version: > EtagVersion.Empty, IsDeleted: false };
    }

    private async Task<App> PrepareAsync(App app)
    {
        if (options.CacheDuration <= TimeSpan.Zero)
        {
            return app;
        }

        // Do not use cancellation here as we already so far.
        await appCache.AddAsync(
        [
            new KeyValuePair<string, object?>(GetCacheKey(app.Id), app),
            new KeyValuePair<string, object?>(GetCacheKey(app.Name), app),
        ], options.CacheDuration);

        return app;
    }

    private Task InvalidateItAsync(DomainId id, string name)
    {
        if (options.CacheDuration <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        // Do not use cancellation here as we already so far.
        return appCache.RemoveAsync(
        [
            GetCacheKey(id),
            GetCacheKey(name)
        ]);
    }
}
