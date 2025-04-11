// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Flows.Internal;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

[OpenApiRequest]
public class FlowDefinitionDto
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
    public Dictionary<Guid, FlowStepDefinitionDto> Steps { get; set; } = [];

    public FlowDefinition ToDefinition()
    {
        return new FlowDefinition
        {
            Steps = Steps?.ToDictionary(
                x => x.Key,
                x => x.Value.ToDefinition())!,
            InitialStep = InitialStep,
        };
    }

    public static FlowDefinitionDto FromDomain(FlowDefinition definition)
    {
        return new FlowDefinitionDto
        {
            Steps = definition.Steps.ToDictionary(
                x => x.Key,
                x => FlowStepDefinitionDto.FromDomain(x.Value)),
            InitialStep = definition.InitialStep,
        };
    }
}
