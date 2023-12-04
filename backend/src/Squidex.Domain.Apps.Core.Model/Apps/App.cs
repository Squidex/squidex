// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Core.Apps;

public record App : Entity
{
    public string Name { get; init; }

    public string Label { get; init; }

    public string Description { get; init; }

    public DomainId? TeamId { get; init; }

    public Contributors Contributors { get; init; } = Contributors.Empty;

    public Roles Roles { get; init; } = Roles.Empty;

    public AssignedPlan? Plan { get; init; }

    public AppClients Clients { get; init; } = AppClients.Empty;

    public AppImage? Image { get; init; }

    public AppSettings Settings { get; init; } = AppSettings.Empty;

    public AssetScripts AssetScripts { get; init; } = new AssetScripts();

    public LanguagesConfig Languages { get; init; } = LanguagesConfig.English;

    public Workflows Workflows { get; init; } = Workflows.Empty;

    public bool IsDeleted { get; init; }

    [Pure]
    public App Annotate(string? label = null, string? description = null)
    {
        var result = this;

        if (label != null && !string.Equals(Label, label, StringComparison.OrdinalIgnoreCase))
        {
            result = result with { Label = label };
        }

        if (description != null && !string.Equals(Description, description, StringComparison.OrdinalIgnoreCase))
        {
            result = result with { Description = description };
        }

        return result;
    }

    [Pure]
    public App Transfer(DomainId? teamId)
    {
        if (Equals(TeamId, teamId))
        {
            return this;
        }

        return this with { TeamId = teamId };
    }

    [Pure]
    public App ChangePlan(AssignedPlan? plan)
    {
        if (Equals(Plan?.PlanId, plan?.PlanId))
        {
            return this;
        }

        return this with { Plan = plan };
    }

    [Pure]
    public App UpdateAssetScripts(AssetScripts? assetScripts)
    {
        if (Equals(AssetScripts, assetScripts) || assetScripts == null)
        {
            return this;
        }

        return this with { AssetScripts = assetScripts };
    }

    [Pure]
    public App UpdateImage(AppImage? image)
    {
        if (Equals(Image, image))
        {
            return this;
        }

        return this with { Image = image };
    }

    [Pure]
    public App UpdateSettings(AppSettings settings)
    {
        if (Equals(Settings, settings) || settings == null)
        {
            return this;
        }

        return this with { Settings = settings };
    }

    [Pure]
    public App UpdateClients<T>(T state, Func<T, AppClients, AppClients> update)
    {
        var newClients = update(state, Clients);

        if (ReferenceEquals(Clients, newClients))
        {
            return this;
        }

        return this with { Clients = newClients };
    }

    [Pure]
    public App UpdateContributors<T>(T state, Func<T, Contributors, Contributors> update)
    {
        var newContributors = update(state, Contributors);

        if (ReferenceEquals(Contributors, newContributors))
        {
            return this;
        }

        return this with { Contributors = newContributors };
    }

    [Pure]
    public App UpdateLanguages<T>(T state, Func<T, LanguagesConfig, LanguagesConfig> update)
    {
        var newLanguages = update(state, Languages);

        if (ReferenceEquals(Languages, newLanguages))
        {
            return this;
        }

        return this with { Languages = newLanguages };
    }

    [Pure]
    public App UpdateRoles<T>(T state, Func<T, Roles, Roles> update)
    {
        var newRoles = update(state, Roles);

        if (ReferenceEquals(Roles, newRoles))
        {
            return this;
        }

        return this with { Roles = newRoles };
    }

    [Pure]
    public App UpdateWorkflows<T>(T state, Func<T, Workflows, Workflows> update)
    {
        var newWorkflows = update(state, Workflows);

        if (ReferenceEquals(Workflows, newWorkflows))
        {
            return this;
        }

        return this with { Workflows = newWorkflows };
    }
}
