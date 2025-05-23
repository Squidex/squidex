﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class WorkflowDto : Resource
{
    /// <summary>
    /// The workflow id.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The name of the workflow.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The workflow steps.
    /// </summary>
    public Dictionary<Status, WorkflowStepDto> Steps { get; set; }

    /// <summary>
    /// The schema ids.
    /// </summary>
    public ReadonlyList<DomainId>? SchemaIds { get; set; }

    /// <summary>
    /// The initial step.
    /// </summary>
    public Status Initial { get; set; }

    public static WorkflowDto FromDomain(DomainId id, Workflow workflow)
    {
        var result = SimpleMapper.Map(workflow, new WorkflowDto
        {
            Steps = workflow.Steps.ToDictionary(
                x => x.Key,
                x => WorkflowStepDto.FromDomain(x.Value)),
            Id = id,
        });

        return result;
    }

    public WorkflowDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App, id = Id };

        if (resources.CanUpdateWorkflow)
        {
            AddPutLink("update",
                resources.Url<AppWorkflowsController>(x => nameof(x.PutWorkflow), values));
        }

        if (resources.CanDeleteWorkflow)
        {
            AddDeleteLink("delete",
                resources.Url<AppWorkflowsController>(x => nameof(x.DeleteWorkflow), values));
        }

        return this;
    }
}
