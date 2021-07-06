// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryById : OperationBase
    {
        public async Task<IContentEntity?> QueryAsync(ISchemaEntity schema, DomainId id,
            CancellationToken ct)
        {
            Guard.NotNull(schema, nameof(schema));

            var documentId = DomainId.Combine(schema.AppId, id);

            var find = Collection.Find(x => x.DocumentId == documentId);

            var contentEntity = await find.FirstOrDefaultAsync(ct);

            if (contentEntity != null)
            {
                if (contentEntity.IndexedSchemaId != schema.Id)
                {
                    return null;
                }
            }

            return contentEntity;
        }
    }
}
