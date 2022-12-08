// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Plans.Models;

public sealed class PlanDto
{
    /// <summary>
    /// The ID of the plan.
    /// </summary>
    [LocalizedRequired]
    public string Id { get; set; }

    /// <summary>
    /// The name of the plan.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    /// <summary>
    /// The monthly costs of the plan.
    /// </summary>
    [LocalizedRequired]
    public string Costs { get; set; }

    /// <summary>
    /// An optional confirm text for the monthly subscription.
    /// </summary>
    public string? ConfirmText { get; set; }

    /// <summary>
    /// An optional confirm text for the yearly subscription.
    /// </summary>
    public string? YearlyConfirmText { get; set; }

    /// <summary>
    /// The yearly costs of the plan.
    /// </summary>
    public string? YearlyCosts { get; set; }

    /// <summary>
    /// The yearly ID of the plan.
    /// </summary>
    public string? YearlyId { get; set; }

    /// <summary>
    /// The maximum number of API traffic.
    /// </summary>
    public long MaxApiBytes { get; set; }

    /// <summary>
    /// The maximum number of API calls.
    /// </summary>
    public long MaxApiCalls { get; set; }

    /// <summary>
    /// The maximum allowed asset size.
    /// </summary>
    public long MaxAssetSize { get; set; }

    /// <summary>
    /// The maximum number of contributors.
    /// </summary>
    public int MaxContributors { get; set; }

    public static PlanDto FromDomain(Plan plan)
    {
        var result = SimpleMapper.Map(plan, new PlanDto());

        return result;
    }
}
