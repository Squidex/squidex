// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps;

public sealed class MongoAppEntity : MongoState<AppDomainObject.State>
{
    [BsonRequired]
    [BsonElement("_an")]
    public string IndexedName { get; set; }

    [BsonRequired]
    [BsonElement("_ui")]
    public string[] IndexedUserIds { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement("_ti")]
    public DomainId? IndexedTeamId { get; set; }

    [BsonRequired]
    [BsonElement("_dl")]
    public bool IndexedDeleted { get; set; }

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
        users.AddRange(Document.Clients.Keys);

        IndexedUserIds = users.ToArray();
        IndexedCreated = Document.Created;
        IndexedDeleted = Document.IsDeleted;
        IndexedTeamId = Document.TeamId;
        IndexedName = Document.Name;
    }
}
