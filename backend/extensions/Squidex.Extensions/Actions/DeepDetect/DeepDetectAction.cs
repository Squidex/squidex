// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Extensions.Actions.DeepDetect;

[Obsolete("Use Flows")]
public sealed record DeepDetectAction : RuleAction
{
    public long MinimumProbability { get; set; }

    public long MaximumTags { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new DeepDetectFlowStep());
    }
}
