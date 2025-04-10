// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Algolia;

public sealed record AlgoliaAction : DeprecatedRuleAction
{
    [LocalizedRequired]
    public string AppId { get; set; }

    [LocalizedRequired]
    public string ApiKey { get; set; }

    [LocalizedRequired]
    public string IndexName { get; set; }

    public string? Document { get; set; }

    public string? Delete { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new AlgoliaFlowStep());
    }
}
