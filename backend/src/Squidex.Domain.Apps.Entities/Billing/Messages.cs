// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Billing;

public sealed record UsageTrackingCheck
{
    public DomainId AppId { get; init; }

    public long Usage { get; init; }

    public long UsageLimit { get; init; }

    public string[] Users { get; init; }
}
