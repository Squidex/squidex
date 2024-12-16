// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RuleFlowContext : FlowContext
{
    required public EnrichedEvent Event { get; set; }

    public JsonObject Shared { get; set; } = [];
}
