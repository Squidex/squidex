// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using MongoDB.Driver;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    public abstract class OperationBase
    {
        protected static readonly SortDefinitionBuilder<MongoContentEntity> Sort = Builders<MongoContentEntity>.Sort;
        protected static readonly UpdateDefinitionBuilder<MongoContentEntity> Update = Builders<MongoContentEntity>.Update;
        protected static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;
        protected static readonly IndexKeysDefinitionBuilder<MongoContentEntity> Index = Builders<MongoContentEntity>.IndexKeys;
        protected static readonly ProjectionDefinitionBuilder<MongoContentEntity> Projection = Builders<MongoContentEntity>.Projection;

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
}
