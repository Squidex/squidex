// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
        var streamFilter = StreamFilter.Prefix("app\\-");

        return rebuilder.RebuildAsync<AppDomainObject, AppDomainObject.State>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildSchemasAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("schema\\-");

        return rebuilder.RebuildAsync<SchemaDomainObject, SchemaDomainObject.State>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildRulesAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("rule\\-");

        return rebuilder.RebuildAsync<RuleDomainObject, RuleDomainObject.State>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildAssetsAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("asset\\-");

        return rebuilder.RebuildAsync<AssetDomainObject, AssetDomainObject.State>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildAssetFoldersAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("assetFolder\\-");

        return rebuilder.RebuildAsync<AssetFolderDomainObject, AssetFolderDomainObject.State>(streamFilter, batchSize, AllowedErrorRate, ct);
    }

    public static Task RebuildContentAsync(this Rebuilder rebuilder, int batchSize,
        CancellationToken ct = default)
    {
        var streamFilter = StreamFilter.Prefix("content\\-");

        return rebuilder.RebuildAsync<ContentDomainObject, ContentDomainObject.State>(streamFilter, batchSize, AllowedErrorRate, ct);
    }
}
