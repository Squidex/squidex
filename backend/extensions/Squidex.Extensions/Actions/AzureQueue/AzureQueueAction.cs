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

namespace Squidex.Extensions.Actions.AzureQueue;

public sealed record AzureQueueAction : DeprecatedRuleAction
{
    [LocalizedRequired]
    public string ConnectionString { get; set; }

    [LocalizedRequired]
    public string Queue { get; set; }

    public string? Payload { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new AzureQueueFlowStep());
    }
}
