// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject
{
    public sealed class SchemaDomainObjectGrain : DomainObjectGrain<SchemaDomainObject, SchemaDomainObject.State>, ISchemaGrain
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
