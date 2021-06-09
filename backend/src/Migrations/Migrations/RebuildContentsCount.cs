// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations
{
    public sealed class RebuildContentsCount : IMigration
    {
        private readonly IContentRepository contentRepository;

        public RebuildContentsCount(IContentRepository contentRepository)
        {
            this.contentRepository = contentRepository;
        }

        public Task UpdateAsync(CancellationToken ct)
        {
            return contentRepository.RebuildCountsAsync(ct);
        }
    }
}
