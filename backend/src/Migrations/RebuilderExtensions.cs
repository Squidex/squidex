// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Migrations;

public static class RebuilderExtensions
{
    private const double AllowedErrorRate = 0.02;

    public static Task RebuildAppsAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("app-");

        return rebuilder.RebuildAsync<AppDomainObject, App>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildSchemasAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("schema-");

        return rebuilder.RebuildAsync<SchemaDomainObject, Schema>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildRulesAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("rule-");

        return rebuilder.RebuildAsync<RuleDomainObject, Rule>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildAssetsAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("asset-");

        return rebuilder.RebuildAsync<AssetDomainObject, Asset>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildAssetFoldersAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("assetFolder-");

        return rebuilder.RebuildAsync<AssetFolderDomainObject, AssetFolder>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildContentAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("content-");

        return rebuilder.RebuildAsync<ContentDomainObject, WriteContent>(streamFilter, batchSize, AllowedErrorRate, ct);
    }
}
