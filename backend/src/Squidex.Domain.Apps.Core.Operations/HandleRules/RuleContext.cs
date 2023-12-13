// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.HandleRules;

public readonly struct RulesContext
{
    public NamedId<DomainId> AppId { get; init; }

    public ReadonlyDictionary<DomainId, Rule> Rules { get; init; }

    public bool IncludeSkipped { get; init; }

    public bool IncludeStale { get; init; }

    public bool AllowExtraEvents { get; init; }

    public int? MaxEvents { get; init; }
}

public readonly struct RuleContext
{
    public NamedId<DomainId> AppId { get; init; }

    public Rule Rule { get; init; }

    public bool IncludeSkipped { get; init; }

    public bool IncludeStale { get; init; }

    public RulesContext ToRulesContext()
    {
        return new RulesContext
        {
            AppId = AppId,
            IncludeSkipped = IncludeSkipped,
            IncludeStale = IncludeStale,
            Rules = new Dictionary<DomainId, Rule>
            {
                [Rule.Id] = Rule
            }.ToReadonlyDictionary()
        };
    }
}
