﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Fastly;

[Obsolete("Has been replaced by flows.")]
public sealed record FastlyAction : RuleAction
{
    [LocalizedRequired]
    public string ApiKey { get; set; }

    [LocalizedRequired]
    public string ServiceId { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new FastlyFlowStep());
    }
}
