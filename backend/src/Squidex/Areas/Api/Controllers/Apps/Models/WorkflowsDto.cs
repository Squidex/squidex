// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class WorkflowsDto : Resource
{
    /// <summary>
    /// The workflow.
    /// </summary>
    [LocalizedRequired]
    public WorkflowDto[] Items { get; set; }

    /// <summary>
    /// The errros that should be fixed.
    /// </summary>
    [LocalizedRequired]
    public string[] Errors { get; set; }

    public static async Task<WorkflowsDto> FromAppAsync(IWorkflowsValidator workflowsValidator, IAppEntity app, Resources resources)
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
