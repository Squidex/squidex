// ==========================================================================
//  MongoExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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
    }
}
