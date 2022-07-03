// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Core;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject
{
    public sealed class SchemaDomainObjectGrain : DomainObjectGrain<SchemaDomainObject, SchemaDomainObject.State>, ISchemaGrain
    {
        public SchemaDomainObjectGrain(IGrainIdentity identity, IDomainObjectFactory factory)
            : base(identity, factory)
        {
        }

        public async Task<ISchemaEntity> GetStateAsync()
        {
            await DomainObject.EnsureLoadedAsync();

            return Snapshot;
        }
    }
}
