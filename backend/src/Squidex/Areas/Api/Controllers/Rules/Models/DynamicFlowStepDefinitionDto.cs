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
public class DynamicFlowStepDefinitionDto
{
    /// <summary>
    /// The actual step.
    /// </summary>
    [LocalizedRequired]
    public Dictionary<string, object> Step { get; set; }

    /// <summary>
    /// The next step.
    /// </summary>
    public Guid NextStepId { get; set; }

    /// <summary>
    /// Indicates if errors should be ignored.
    /// </summary>
    public bool IgnoreError { get; set; }
}
