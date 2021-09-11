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
        [BsonElement]
        public string IndexedName { get; set; }

        [BsonRequired]
        [BsonElement]
        public string[] IndexedContributorIds { get; set; }

        public override void Prepare()
        {
            IndexedName = Document.Name;
            IndexedContributorIds = Document.Contributors.Keys.ToArray();
        }
    }
}
