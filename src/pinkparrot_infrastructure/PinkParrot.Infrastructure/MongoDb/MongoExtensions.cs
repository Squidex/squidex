// ==========================================================================
//  MongoExtensions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;

namespace PinkParrot.Infrastructure.MongoDb
{
    public static class MongoExtensions
    {
        public static async Task<bool> InsertOneIfExistsAsync<T>(this IMongoCollection<T> collection, T document)
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
