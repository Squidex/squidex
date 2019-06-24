// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetEnricher
    {
        Task<IAssetEntityEnriched> EnrichAsync(IAssetEntity asset);

        Task<IReadOnlyList<IAssetEntityEnriched>> EnrichAsync(IEnumerable<IAssetEntity> assets);
    }
}