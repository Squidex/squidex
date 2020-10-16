// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryContent : OperationBase
    {
        private readonly DataConverter converter;

        public QueryContent(DataConverter converter)
        {
            this.converter = converter;
        }

        public async Task<IContentEntity?> DoAsync(ISchemaEntity schema, DomainId id)
        {
            Guard.NotNull(schema, nameof(schema));

            var documentId = DomainId.Combine(schema.AppId, id);

            var find = Collection.Find(x => x.DocumentId == documentId);

            var contentEntity = await find.FirstOrDefaultAsync();

            if (contentEntity != null)
            {
                if (contentEntity.IndexedSchemaId != schema.Id)
                {
                    return null;
                }

                contentEntity?.ParseData(schema.SchemaDef, converter);
            }

            return contentEntity;
        }
    }
}
