// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetItemGrain : IDomainObjectGrain
    {
        Task<J<IAssetItemEntity>> GetStateAsync(long version = EtagVersion.Any);
    }
}
