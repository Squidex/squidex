// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Triggers;

[TypeName(nameof(CronJobTrigger))]
public sealed record CronJobTrigger : RuleTrigger
{
    public string CronExpression { get; init; }

    public string? CronTimezone { get; init; }

    public JsonValue Value { get; init; }

    public override T Accept<T, TArgs>(IRuleTriggerVisitor<T, TArgs> visitor, TArgs args)
    {
        return visitor.Visit(this, args);
    }
}
