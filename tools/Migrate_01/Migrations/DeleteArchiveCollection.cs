// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.Migrations
{
    public sealed class DeleteArchiveCollection : IMigration
    {
        private readonly IContentRepository contentRepository;

        public DeleteArchiveCollection(IContentRepository contentRepository)
        {
            this.contentRepository = contentRepository;
        }

        public async Task UpdateAsync()
        {
            if (contentRepository is MongoContentRepository mongoContentRepository)
            {
                await mongoContentRepository.DeleteArchiveAsync();
            }
        }
    }
}
