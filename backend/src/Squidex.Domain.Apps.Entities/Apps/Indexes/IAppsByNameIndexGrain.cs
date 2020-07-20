// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans.Indexes;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public interface IAppsByNameIndexGrain : IUniqueNameIndexGrain<DomainId>, IGrainWithStringKey
    {
    }
}
