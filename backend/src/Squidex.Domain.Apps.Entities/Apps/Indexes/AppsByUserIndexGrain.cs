// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Orleans.Indexes;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByUserIndexGrain : IdsIndexGrain<AppsByUserIndex, DomainId>, IAppsByUserIndexGrain
    {
        public AppsByUserIndexGrain(IGrainState<AppsByUserIndex> state)
            : base(state)
        {
        }
    }

    [CollectionName("Index_AppsByUser")]
    public sealed class AppsByUserIndex : IdsIndexState<DomainId>
    {
    }
}
