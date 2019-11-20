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
        Task<IEnrichedAssetItemEntity> EnrichAsync(IAssetItemEntity asset, Context context);

        Task<IReadOnlyList<IEnrichedAssetItemEntity>> EnrichAsync(IEnumerable<IAssetItemEntity> assets, Context context);
    }
}