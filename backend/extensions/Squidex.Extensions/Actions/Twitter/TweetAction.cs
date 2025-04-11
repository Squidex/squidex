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

namespace Squidex.Extensions.Actions.Twitter;

[Obsolete("Use Flows")]
public sealed record TweetAction : RuleAction
{
    [LocalizedRequired]
    public string AccessToken { get; set; }

    [LocalizedRequired]
    public string AccessSecret { get; set; }

    [LocalizedRequired]
    public string Text { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new TweetFlowStep());
    }
}
