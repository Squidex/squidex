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

namespace Squidex.Extensions.Actions.Comment;

public sealed record CommentAction : DeprecatedRuleAction
{
    [LocalizedRequired]
    public string Text { get; set; }

    public string? Client { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new CommentFlowStep());
    }
}
