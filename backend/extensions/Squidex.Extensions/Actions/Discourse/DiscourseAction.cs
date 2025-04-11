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

namespace Squidex.Extensions.Actions.Discourse;

[Obsolete("Use Flows")]
public sealed record DiscourseAction : RuleAction
{
    [AbsoluteUrl]
    [LocalizedRequired]
    public Uri Url { get; set; }

    [LocalizedRequired]
    public string ApiKey { get; set; }

    [LocalizedRequired]
    public string ApiUsername { get; set; }

    [LocalizedRequired]
    public string Text { get; set; }

    public string? Title { get; set; }

    public int? Topic { get; set; }

    public int? Category { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new DiscourseFlowStep());
    }
}
