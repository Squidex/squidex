﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Flows.Steps;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Script;

[Obsolete("Use Flows")]
public sealed record ScriptAction : RuleAction
{
    [LocalizedRequired]
    public string Script { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new ScriptFlowStepBase());
    }
}
