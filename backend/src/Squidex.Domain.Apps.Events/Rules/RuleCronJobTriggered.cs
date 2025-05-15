// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Events.Rules;

public sealed class RuleCronJobTriggered : RuleEvent
{
    public JsonValue Value { get; set; }
}
