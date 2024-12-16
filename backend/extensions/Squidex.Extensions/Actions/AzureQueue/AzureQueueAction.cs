// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Extensions.Actions.AzureQueue;

public sealed record AzureQueueAction : RuleAction<AzureQueueStep>
{
    public string ConnectionString { get; set; }

    public string Queue { get; set; }

    public string? Payload { get; set; }
}
