// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.SignalR;

public sealed record SignalRAction : RuleAction<SignalRStep>
{
    public ActionTypeEnum Action { get; set; }

    public string ConnectionString { get; set; }

    public string HubName { get; set; }

    public string? MethodName { get; set; }

    public string? Target { get; set; }

    public string? Payload { get; set; }
}
