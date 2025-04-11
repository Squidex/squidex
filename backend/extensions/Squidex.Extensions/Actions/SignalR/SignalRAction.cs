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

namespace Squidex.Extensions.Actions.SignalR;

[Obsolete("Use Flows")]
public sealed record SignalRAction : RuleAction
{
    [LocalizedRequired]
    public string ConnectionString { get; set; }

    [LocalizedRequired]
    public string HubName { get; set; }

    public SignalRActionType Action { get; set; }

    public string? MethodName { get; set; }

    public string? Target { get; set; }

    public string? Payload { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new SignalRFlowStep());
    }
}
