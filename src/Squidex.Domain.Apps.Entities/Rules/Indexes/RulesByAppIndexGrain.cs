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

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public sealed class RulesByAppIndexGrain : IdsIndexGrain<RulesByAppIndexState, Guid>, IRulesByAppIndexGrain
    {
        public RulesByAppIndexGrain(IGrainState<RulesByAppIndexState> state)
            : base(state)
        {
        }
    }

    [CollectionName("Index_RulesByApp")]
    public sealed class RulesByAppIndexState : IdsIndexState<Guid>
    {
    }
}
