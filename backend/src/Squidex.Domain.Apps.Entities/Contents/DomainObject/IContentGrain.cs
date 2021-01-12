// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public interface IContentGrain : IDomainObjectGrain
    {
        Task<J<IContentEntity>> GetStateAsync(long version = EtagVersion.Any);
    }
}
