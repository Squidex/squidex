// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class FlowEventContext : FlowContext
{
    public EnrichedEvent Event { get; init; }
}
