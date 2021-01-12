// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject
{
    public interface ISchemaGrain : IDomainObjectGrain
    {
        Task<J<ISchemaEntity>> GetStateAsync();
    }
}
