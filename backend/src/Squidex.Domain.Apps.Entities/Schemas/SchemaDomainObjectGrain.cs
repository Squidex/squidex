// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class SchemaDomainObjectGrain : DomainObjectGrain<SchemaDomainObject, SchemaState>, ISchemaGrain
    {
        public SchemaDomainObjectGrain(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public async Task<J<ISchemaEntity>> GetStateAsync()
        {
            await DomainObject.EnsureLoadedAsync();

            return Snapshot;
        }
    }
}
