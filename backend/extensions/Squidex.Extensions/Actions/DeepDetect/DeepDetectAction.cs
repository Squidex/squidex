// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Extensions.Actions.DeepDetect;

public sealed record DeepDetectAction : RuleAction<DeepDetectStep>
{
    public long MinimumProbability { get; set; }

    public long MaximumTags { get; set; }

    public override IFlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new DeepDetectStep());
    }
}
