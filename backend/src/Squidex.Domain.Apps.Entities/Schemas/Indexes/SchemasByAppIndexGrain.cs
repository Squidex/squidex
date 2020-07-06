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

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public sealed class SchemasByAppIndexGrain : UniqueNameIndexGrain<SchemasByAppIndexGrainState, DomainId>, ISchemasByAppIndexGrain
    {
        public SchemasByAppIndexGrain(IGrainState<SchemasByAppIndexGrainState> state)
            : base(state)
        {
        }
    }

    [CollectionName("Index_SchemasByApp")]
    public sealed class SchemasByAppIndexGrainState : UniqueNameIndexState<DomainId>
    {
    }
}
