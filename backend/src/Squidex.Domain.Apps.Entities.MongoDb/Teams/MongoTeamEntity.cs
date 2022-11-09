// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Entities.Teams.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Teams;

public sealed class MongoTeamEntity : MongoState<TeamDomainObject.State>
{
    [BsonRequired]
    [BsonElement("_ui")]
    public string[] IndexedUserIds { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement("_ct")]
    public Instant IndexedCreated { get; set; }

    public override void Prepare()
    {
        var users = new HashSet<string>
        {
            Document.CreatedBy.Identifier
        };

        users.AddRange(Document.Contributors.Keys);

        IndexedUserIds = users.ToArray();
    }
}
