﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Infrastructure.Commands;

namespace Migrations
{
    public static class RebuilderExtensions
    {
        public static Task RebuildAppsAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<AppDomainObject, AppDomainObject.State>("^app\\-", ct);
        }

        public static Task RebuildSchemasAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<SchemaDomainObject, SchemaDomainObject.State>("^schema\\-", ct);
        }

        public static Task RebuildRulesAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<RuleDomainObject, RuleDomainObject.State>("^rule\\-", ct);
        }

        public static Task RebuildAssetsAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<AssetDomainObject, AssetDomainObject.State>("^asset\\-", ct);
        }

        public static Task RebuildAssetFoldersAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<AssetFolderDomainObject, AssetFolderDomainObject.State>("^assetFolder\\-", ct);
        }

        public static Task RebuildContentAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<ContentDomainObject, ContentDomainObject.State>("^content\\-", ct);
        }
    }
}