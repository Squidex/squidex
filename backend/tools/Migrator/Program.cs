// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Migrator
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Migrate EventStore");

            var mongoClient = new MongoClient(GetMongoConnectionValue());
            var mongoDatabase = mongoClient.GetDatabase(GetMongoDatabaseValue());

            var collection = mongoDatabase.GetCollection<BsonDocument>("Events");

            Console.Write("Migrate indexes.....");

            collection.Indexes.DropAll();

            Console.WriteLine("DONE");

            var query =
                collection.Find(new BsonDocument())
                    .Project<BsonDocument>(
                        Builders<BsonDocument>.Projection.Include(Field("EventsOffset")))
                    .ToList();

            Console.Write("Migrate Documents...");

            foreach (var eventCommit in query)
            {
                var eventsOffset = (int)eventCommit["EventsOffset"].AsInt64;

                var ts = new BsonTimestamp(eventsOffset + 10, 1);

                collection.UpdateOne(
                    Builders<BsonDocument>.Filter
                        .Eq(Field<string>("_id"), eventCommit["_id"].AsString),
                    Builders<BsonDocument>.Update
                        .Set(Field<BsonTimestamp>("Timestamp"), ts).Unset(Field("EventsOffset")));
            }

            Console.WriteLine("DONE");
        }

        private static StringFieldDefinition<BsonDocument, T> Field<T>(string fieldName)
        {
            return new StringFieldDefinition<BsonDocument, T>(fieldName);
        }

        private static StringFieldDefinition<BsonDocument> Field(string fieldName)
        {
            return new StringFieldDefinition<BsonDocument>(fieldName);
        }

        private static string GetMongoConnectionValue()
        {
            Console.Write("Mongo Connection (ENTER for 'mongodb://localhost'): ");

            var mongoConnection = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(mongoConnection))
            {
                mongoConnection = "mongodb://localhost";
            }

            return mongoConnection;
        }

        private static string GetMongoDatabaseValue()
        {
            Console.Write("Mongo Database (ENTER for 'Squidex'): ");

            var mongoDatabase = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(mongoDatabase))
            {
                mongoDatabase = "Squidex";
            }

            return mongoDatabase;
        }
    }
}
