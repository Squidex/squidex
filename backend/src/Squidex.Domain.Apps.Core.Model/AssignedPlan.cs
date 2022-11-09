// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core;

public sealed record AssignedPlan(RefToken Owner, string PlanId)
{
    public RefToken Owner { get; } = Guard.NotNull(Owner);

    public string PlanId { get; } = Guard.NotNullOrEmpty(PlanId);
}
