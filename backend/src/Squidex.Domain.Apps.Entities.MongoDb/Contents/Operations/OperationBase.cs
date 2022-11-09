// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

public abstract class OperationBase : MongoBase<MongoContentEntity>
{
    public IMongoCollection<MongoContentEntity> Collection { get; private set; }

    public void Setup(IMongoCollection<MongoContentEntity> collection)
    {
        Collection = collection;
    }

    public virtual IEnumerable<CreateIndexModel<MongoContentEntity>> CreateIndexes()
    {
        yield break;
    }
}
