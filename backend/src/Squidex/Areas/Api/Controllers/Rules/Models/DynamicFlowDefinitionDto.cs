// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

[OpenApiRequest]
public class DynamicFlowDefinitionDto
{
    /// <summary>
    /// The ID of the initial step.
    /// </summary>
    [LocalizedRequired]
    public Guid InitialStep { get; set; }

    /// <summary>
    /// The steps.
    /// </summary>
    [LocalizedRequired]
    public Dictionary<Guid, DynamicFlowStepDefinitionDto> Steps { get; set; } = [];
}
