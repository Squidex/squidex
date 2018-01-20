// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoXmlDocument
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Xml { get; set; }
    }
}
