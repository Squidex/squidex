// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure.Commands;

namespace Migrations
{
    public static class RebuilderExtensions
    {
        public static Task RebuildAppsAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<AppDomainObject, AppState>("^app\\-", ct);
        }

        public static Task RebuildSchemasAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<SchemaDomainObject, SchemaState>("^schema\\-", ct);
        }

        public static Task RebuildRulesAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<RuleDomainObject, RuleState>("^rule\\-", ct);
        }

        public static Task RebuildAssetsAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<AssetDomainObject, AssetState>("^asset\\-", ct);
        }

        public static Task RebuildAssetFoldersAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<AssetFolderDomainObject, AssetFolderState>("^assetFolder\\-", ct);
        }

        public static Task RebuildContentAsync(this Rebuilder rebuilder, CancellationToken ct = default)
        {
            return rebuilder.RebuildAsync<ContentDomainObject, ContentState>("^content\\-", ct);
        }
    }
}