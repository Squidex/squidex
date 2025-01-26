// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations.Schema;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Teams;

public sealed class EFTeamEntity : EFState<Team>
{
    [Column("UserIds")]
    public string IndexedUserIds { get; set; }

    [Column("Deleted")]
    public bool IndexedDeleted { get; set; }

    [Column("AuthDomain")]
    public string? IndexedAuthDomain { get; set; }

    public override void Prepare()
    {
        var users = new HashSet<string>
        {
            Document.CreatedBy.Identifier,
        };

        users.AddRange(Document.Contributors.Keys);

        IndexedAuthDomain = Document.AuthScheme?.Domain;
        IndexedDeleted = Document.IsDeleted;
        IndexedUserIds = TagsConverter.ToString(users);
    }
}
