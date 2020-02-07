// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentDomainObjectGrain : DomainObjectGrain<ContentDomainObject, ContentState>, IContentGrain
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

        public ContentDomainObjectGrain(IServiceProvider serviceProvider, IActivationLimit limit)
            : base(serviceProvider)
        {
            limit?.SetLimit(5000, Lifetime);
        }

        public async Task<J<IContentEntity>> GetStateAsync(long version = -2)
        {
            await DomainObject.EnsureLoadedAsync();

            return await GetStateAsync(version);
        }
    }
}
