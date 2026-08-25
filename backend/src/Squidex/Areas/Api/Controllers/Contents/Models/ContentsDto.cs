// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

public sealed class ContentsDto : Resource
{
    /// <summary>
    /// The total number of content items.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// The content items.
    /// </summary>
    public ContentDto[] Items { get; set; }

    /// <summary>
    /// The possible statuses.
    /// </summary>
    public StatusInfoDto[] Statuses { get; set; }

    public static async Task<ContentsDto> FromContentsAsync(IResultList<EnrichedContent> contents, Resources resources,
        Schema? schema, IContentWorkflows workflows)
    {
        var result = new ContentsDto
        {
            Total = contents.Total,
            Items = contents.Select(x => ContentDto.FromDomain(x, resources)).ToArray(),
        };

        if (schema != null)
        {
            using var workflow = await workflows.GetWorkflowAsync(resources.Context.App, schema);

            result.CreateStatuses(workflow);
            result.CreateLinks(resources, workflow, schema);
        }
        else
        {
            result.Statuses = [];
        }

        return result;
    }

    private void CreateStatuses(IContentWorkflow workflow)
    {
        Statuses = workflow.GetAll().Select(StatusInfoDto.FromDomain).ToArray();
    }

    private void CreateLinks(Resources resources, IContentWorkflow workflow, Schema schema)
    {
        var values = new { app = resources.App, schema = schema.Name };

        AddSelfLink(resources.Url<ContentsController>(x => nameof(x.GetContents), values));

        if (resources.CanCreateContent(values.schema))
        {
            AddPostLink("create",
                resources.Url<ContentsController>(x => nameof(x.PostContent), values));

            if (resources.CanChangeStatus(values.schema) && workflow.CanPublishInitial(resources.Context.UserPrincipal))
            {
                var publishValues = new { values.app, values.schema, publish = true };

                AddPostLink("create/publish",
                    resources.Url<ContentsController>(x => nameof(x.PostContent), publishValues));
            }
        }
    }
}
