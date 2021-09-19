// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas
{
    public sealed class MongoSchemaEntity : MongoState<SchemaDomainObject.State>
    {
        [BsonRequired]
        [BsonElement("_ai")]
        public DomainId IndexedAppId { get; set; }

        [BsonRequired]
        [BsonElement("_si")]
        public DomainId IndexedId { get; set; }

        [BsonRequired]
        [BsonElement("_sn")]
        public string IndexedName { get; set; }

        [BsonRequired]
        [BsonElement("_dl")]
        public bool IndexedDeleted { get; set; }

        public override void Prepare()
        {
            IndexedAppId = Document.AppId.Id;
            IndexedDeleted = Document.IsDeleted;
            IndexedId = Document.Id;
            IndexedName = Document.SchemaDef.Name;
        }
    }
}
