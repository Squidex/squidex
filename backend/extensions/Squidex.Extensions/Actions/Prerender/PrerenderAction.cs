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

namespace Squidex.Extensions.Actions.Prerender;

[Obsolete("Has been replaced by flows.")]
public sealed record PrerenderAction : RuleAction
{
    [LocalizedRequired]
    public string Token { get; set; }

    [LocalizedRequired]
    public string Url { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new PrerenderFlowStep());
    }
}
