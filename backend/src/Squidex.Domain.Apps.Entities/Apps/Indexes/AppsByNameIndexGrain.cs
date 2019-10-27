// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Orleans.Indexes;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByNameIndexGrain : UniqueNameIndexGrain<AppsByNameIndexState, Guid>, IAppsByNameIndexGrain
    {
        public AppsByNameIndexGrain(IGrainState<AppsByNameIndexState> state)
            : base(state)
        {
        }
    }

    [CollectionName("Index_AppsByName")]
    public sealed class AppsByNameIndexState : UniqueNameIndexState<Guid>
    {
    }
}
