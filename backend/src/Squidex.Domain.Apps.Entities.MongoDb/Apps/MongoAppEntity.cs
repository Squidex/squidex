// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps
{
    public sealed class MongoAppEntity : MongoState<AppDomainObject.State>
    {
        [BsonRequired]
        [BsonElement("_an")]
        public string IndexedName { get; set; }

        [BsonRequired]
        [BsonElement("_ci")]
        public string[] IndexedContributorIds { get; set; }

        [BsonRequired]
        [BsonElement("_dl")]
        public bool IndexedDeleted { get; set; }

        public override void Prepare()
        {
            IndexedContributorIds = Document.Contributors.Keys.ToArray();
            IndexedDeleted = Document.IsDeleted;
            IndexedName = Document.Name;
        }
    }
}
