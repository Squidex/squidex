// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Core;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public sealed class ContentDomainObjectGrain : DomainObjectGrain<ContentDomainObject, ContentDomainObject.State>, IContentGrain
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

        public ContentDomainObjectGrain(IGrainIdentity identity, IDomainObjectFactory factory, IActivationLimit limit)
            : base(identity, factory)
        {
            limit?.SetLimit(5000, Lifetime);
        }

        public async Task<IContentEntity> GetStateAsync(long version = -2)
        {
            await DomainObject.EnsureLoadedAsync();

            return await DomainObject.GetSnapshotAsync(version);
        }
    }
}
