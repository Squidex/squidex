﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations.Schema;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class EFRuleEntity : EFState<Rule>
{
    [Column("AppId")]
    public DomainId IndexedAppId { get; set; }

    [Column("Id")]
    public DomainId IndexedId { get; set; }

    [Column("Deleted")]
    public bool IndexedDeleted { get; set; }

    public override void Prepare()
    {
        IndexedAppId = Document.AppId.Id;
        IndexedDeleted = Document.IsDeleted;
        IndexedId = Document.Id;
    }
}
