// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

[OpenApiRequest]
public class FlowStepDefinitionDto
{
    /// <summary>
    /// The actual step.
    /// </summary>
    [LocalizedRequired]
    public FlowStep Step { get; set; }

    /// <summary>
    /// The optional descriptive name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The next step.
    /// </summary>
    public Guid? NextStepId { get; set; }

    /// <summary>
    /// Indicates if errors should be ignored.
    /// </summary>
    public bool IgnoreError { get; set; }

    public FlowStepDefinition ToDefinition()
    {
        return SimpleMapper.Map(this, new FlowStepDefinition());
    }

    public static FlowStepDefinitionDto FromDomain(FlowStepDefinition stepDefinition)
    {
        return SimpleMapper.Map(stepDefinition, new FlowStepDefinitionDto());
    }
}
