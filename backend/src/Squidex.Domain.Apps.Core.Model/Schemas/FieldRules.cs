// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed class FieldRules : ReadonlyList<FieldRule>
{
    public static readonly new FieldRules Empty = new FieldRules(new List<FieldRule>());

    public FieldRules()
    {
    }

    public FieldRules(IList<FieldRule> list)
        : base(list)
    {
    }

    public static FieldRules Create(params FieldRule[]? rules)
    {
        return rules is not { Length: > 0 } ? Empty : new FieldRules(rules.ToArray());
    }
}
