// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans.Indexes;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public interface ISchemasByAppIndexGrain : IUniqueNameIndexGrain<DomainId>, IGrainWithStringKey
    {
    }
}
