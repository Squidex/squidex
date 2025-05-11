// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

public sealed class EnrichedCronJobEvent : EnrichedUserEventBase
{
    public JsonValue Value { get; set; }

    public override long Partition
    {
        get => 0;
    }
}
