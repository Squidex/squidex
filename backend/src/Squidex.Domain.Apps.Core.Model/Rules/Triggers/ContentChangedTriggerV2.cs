// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Triggers;

[TypeName(nameof(ContentChangedTriggerV2))]
public sealed record ContentChangedTriggerV2 : RuleTrigger
{
    public ReadonlyList<SchemaCondition>? Schemas { get; init; }

    public ReadonlyList<SchemaCondition>? ReferencedSchemas { get; init; }

    public bool HandleAll { get; init; }

    public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
