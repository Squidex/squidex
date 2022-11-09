// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

internal sealed class QueryById : OperationBase
{
    public async Task<IContentEntity?> QueryAsync(ISchemaEntity schema, DomainId id,
        CancellationToken ct)
    {
        var filter = Filter.Eq(x => x.DocumentId, DomainId.Combine(schema.AppId, id));

        var contentEntity = await Collection.Find(filter).FirstOrDefaultAsync(ct);

        if (contentEntity == null || contentEntity.IndexedSchemaId != schema.Id)
        {
            return null;
        }

        return contentEntity;
    }
}
