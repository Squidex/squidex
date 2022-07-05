// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public interface IContentGrain : IDomainObjectGrain
    {
        Task<IContentEntity> GetStateAsync(long version = EtagVersion.Any);
    }
}
