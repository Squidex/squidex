// ==========================================================================
//  MongoXmlRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
        private static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };

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
            Collection.UpdateOne(Filter.Eq(x => x.Id, friendlyName),
                Update.Set(x => x.Xml, element.ToString()),
                Upsert);
        }
    }
}
