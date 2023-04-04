// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Plans.Models;

[OpenApiRequest]
public sealed class ChangePlanDto
{
    /// <summary>
    /// The new plan id.
    /// </summary>
    [LocalizedRequired]
    public string PlanId { get; set; }
}
