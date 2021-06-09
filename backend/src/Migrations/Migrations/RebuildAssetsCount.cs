// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations
{
    public sealed class RebuildAssetsCount : IMigration
    {
        private readonly IAssetRepository assetRepository;

        public RebuildAssetsCount(IAssetRepository assetRepository)
        {
            this.assetRepository = assetRepository;
        }

        public Task UpdateAsync(CancellationToken ct)
        {
            return assetRepository.RebuildCountsAsync(ct);
        }
    }
}
