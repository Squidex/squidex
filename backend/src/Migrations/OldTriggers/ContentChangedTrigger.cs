// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldTriggers;

[TypeName(nameof(ContentChangedTrigger))]
public sealed record ContentChangedTrigger : RuleTrigger, IMigrated<RuleTrigger>
{
    public ReadonlyList<ContentChangedTriggerSchema> Schemas { get; set; }

    public bool HandleAll { get; set; }

    public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
    {
        throw new NotSupportedException();
    }

    public RuleTrigger Migrate()
    {
        var schemas = Schemas.Select(x => x.Migrate()).ToReadonlyList();

        return new ContentChangedTriggerV2 { HandleAll = HandleAll, Schemas = schemas };
    }
}
