// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject
{
    public interface ISchemaGrain : IDomainObjectGrain
    {
        Task<ISchemaEntity> GetStateAsync();
    }
}
