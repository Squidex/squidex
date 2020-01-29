// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    public abstract class OperationBase
    {
        protected static readonly SortDefinitionBuilder<MongoContentEntity> Sort = Builders<MongoContentEntity>.Sort;
        protected static readonly UpdateDefinitionBuilder<MongoContentEntity> Update = Builders<MongoContentEntity>.Update;
        protected static readonly FieldDefinitionBuilder<MongoContentEntity> Fields = FieldDefinitionBuilder<MongoContentEntity>.Instance;
        protected static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;
        protected static readonly IndexKeysDefinitionBuilder<MongoContentEntity> Index = Builders<MongoContentEntity>.IndexKeys;
        protected static readonly ProjectionDefinitionBuilder<MongoContentEntity> Projection = Builders<MongoContentEntity>.Projection;

        public IMongoCollection<MongoContentEntity> Collection { get; }

        protected OperationBase(IMongoCollection<MongoContentEntity> collection)
        {
            Collection = collection;
        }

        public virtual Task PrepareAsync(CancellationToken ct = default)
        {
            return TaskHelper.Done;
        }
    }
}
