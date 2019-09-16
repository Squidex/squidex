// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;
using Squidex.Infrastructure.Orleans.Indexes;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public interface IRulesByAppIndexGrain : IIdsIndexGrain<Guid>, IGrainWithGuidKey
    {
    }
}
