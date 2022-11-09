// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class UpdateWorkflowDto
{
    /// <summary>
    /// The name of the workflow.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The workflow steps.
    /// </summary>
    [LocalizedRequired]
    public Dictionary<Status, WorkflowStepDto> Steps { get; set; }

    /// <summary>
    /// The schema ids.
    /// </summary>
    public ReadonlyList<DomainId>? SchemaIds { get; set; }

    /// <summary>
    /// The initial step.
    /// </summary>
    [LocalizedRequired]
    public Status Initial { get; set; }

    public UpdateWorkflow ToCommand(DomainId id)
    {
        var workflow = new Workflow(
            Initial,
            Steps?.ToReadonlyDictionary(
                x => x.Key,
                x => x.Value?.ToWorkflowStep()!),
            SchemaIds,
            Name);

        return new UpdateWorkflow { WorkflowId = id, Workflow = workflow };
    }
}
