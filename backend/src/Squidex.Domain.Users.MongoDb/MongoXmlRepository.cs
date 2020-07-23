// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoXmlRepository : IXmlRepository
    {
        private static readonly ReplaceOptions UpsertReplace = new ReplaceOptions { IsUpsert = true };
        private readonly IMongoCollection<MongoXmlEntity> collection;

        public MongoXmlRepository(IMongoDatabase mongoDatabase)
        {
            Guard.NotNull(mongoDatabase, nameof(mongoDatabase));

            collection = mongoDatabase.GetCollection<MongoXmlEntity>("States_Repository");
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var documents = collection.Find(new BsonDocument()).ToList();

            var elements = documents.Select(x => XElement.Parse(x.Xml)).ToList();

            return elements;
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            var document = new MongoXmlEntity
            {
                FriendlyName = friendlyName
            };

            document.Xml = element.ToString();

            collection.ReplaceOne(x => x.FriendlyName == friendlyName, document, UpsertReplace);
        }
    }
}
