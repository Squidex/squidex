// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Comments.DomainObject;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentsLoader : ICommentsLoader
    {
        private readonly Func<DomainId, CommentsStream> factory;

        public CommentsLoader(IServiceProvider serviceProvider)
        {
            var objectFactory = ActivatorUtilities.CreateFactory(typeof(CommentsStream), new[] { typeof(DomainId) });

            factory = key =>
            {
                return (CommentsStream)objectFactory(serviceProvider, new object[] { key });
            };
        }

        public async Task<CommentsResult> GetCommentsAsync(DomainId id, long version = EtagVersion.Any,
            CancellationToken ct = default)
        {
            var stream = factory(id);

            await stream.LoadAsync(ct);

            return stream.GetComments(version);
        }
    }
}
