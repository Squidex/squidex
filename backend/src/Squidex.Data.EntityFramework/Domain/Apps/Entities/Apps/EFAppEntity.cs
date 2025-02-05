// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations.Schema;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class EFAppEntity : EFState<App>
{
    [Column("Name")]
    public string IndexedName { get; set; }

    [Column("UserIds")]
    public string IndexedUserIds { get; set; }

    [Column("TeamId")]
    public DomainId? IndexedTeamId { get; set; }

    [Column("Deleted")]
    public bool IndexedDeleted { get; set; }

    [Column("Created")]
    public DateTimeOffset IndexedCreated { get; set; }

    public override void Prepare()
    {
        var users = new HashSet<string>
        {
            Document.CreatedBy.Identifier,
        };

        users.AddRange(Document.Contributors.Keys);
        users.AddRange(Document.Clients.Keys);

        IndexedCreated = Document.Created.ToDateTimeOffset();
        IndexedDeleted = Document.IsDeleted;
        IndexedName = Document.Name;
        IndexedTeamId = Document.TeamId;
        IndexedUserIds = TagsConverter.ToString(users);
    }
}
