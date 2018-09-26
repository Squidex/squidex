// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoXmlRepository : MongoRepositoryBase<MongoXmlDocument>, IXmlRepository
    {
        public MongoXmlRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Identity_XmlRepository";
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var elements = Collection.Find(new BsonDocument()).ToList();

            return elements.Select(x => XElement.Parse(x.Xml)).ToList();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            Collection.UpdateOne(x => x.Id == friendlyName,
                Update.Set(x => x.Xml, element.ToString()),
                Upsert);
        }
    }
}
