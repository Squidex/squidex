// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class WorkflowsDto : Resource
{
    /// <summary>
    /// The workflow.
    /// </summary>
    public WorkflowDto[] Items { get; set; }

    /// <summary>
    /// The errros that should be fixed.
    /// </summary>
    public string[] Errors { get; set; }

    public static async Task<WorkflowsDto> FromAppAsync(IWorkflowsValidator workflowsValidator, App app, Resources resources)
    {
        var result = new WorkflowsDto
        {
            Items =
                app.Workflows
                    .Select(x => WorkflowDto.FromDomain(x.Key, x.Value))
                    .Select(x => x.CreateLinks(resources))
                    .ToArray()
        };

        var errors = await workflowsValidator.ValidateAsync(app.Id, app.Workflows);

        result.Errors = errors.ToArray();

        return result.CreateLinks(resources);
    }

    private WorkflowsDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App };

        AddSelfLink(resources.Url<AppWorkflowsController>(x => nameof(x.GetWorkflows), values));

        if (resources.CanCreateWorkflow)
        {
            AddPostLink("create",
                resources.Url<AppWorkflowsController>(x => nameof(x.PostWorkflow), values));
        }

        return this;
    }
}
