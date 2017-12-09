// ==========================================================================
//  MongoExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Squidex.Infrastructure.MongoDb
{
    public static class MongoExtensions
    {
        public static async Task<bool> InsertOneIfNotExistsAsync<T>(this IMongoCollection<T> collection, T document)
        {
            try
            {
                await collection.InsertOneAsync(document);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return false;
                }

                throw;
            }

            return true;
        }

        public static IFindFluent<TDocument, TDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object>> include)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Include(include));
        }

        public static IFindFluent<TDocument, TDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object>> include1,
            Expression<Func<TDocument, object>> include2)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Include(include1).Include(include2));
        }

        public static IFindFluent<TDocument, TDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object>> include1,
            Expression<Func<TDocument, object>> include2,
            Expression<Func<TDocument, object>> include3)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Include(include1).Include(include2).Include(include3));
        }
    }
}
