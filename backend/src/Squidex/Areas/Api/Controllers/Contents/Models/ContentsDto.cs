// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
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
    [LocalizedRequired]
    public ContentDto[] Items { get; set; }

    /// <summary>
    /// The possible statuses.
    /// </summary>
    [LocalizedRequired]
    public StatusInfoDto[] Statuses { get; set; }

    public static async Task<ContentsDto> FromContentsAsync(IResultList<IEnrichedContentEntity> contents, Resources resources,
        ISchemaEntity? schema, IContentWorkflow workflow)
    {
        var result = new ContentsDto
        {
            Total = contents.Total,
            Items = contents.Select(x => ContentDto.FromDomain(x, resources)).ToArray()
        };

        if (schema != null)
        {
            await result.AssignStatusesAsync(workflow, schema);

            await result.CreateLinksAsync(resources, workflow, schema);
        }

        return result;
    }

    private async Task AssignStatusesAsync(IContentWorkflow workflow, ISchemaEntity schema)
    {
        var allStatuses = await workflow.GetAllAsync(schema);

        Statuses = allStatuses.Select(StatusInfoDto.FromDomain).ToArray();
    }

    private async Task CreateLinksAsync(Resources resources, IContentWorkflow workflow, ISchemaEntity schema)
    {
        var values = new { app = resources.App, schema = schema.SchemaDef.Name };

        AddSelfLink(resources.Url<ContentsController>(x => nameof(x.GetContents), values));

        if (resources.CanCreateContent(values.schema))
        {
            AddPostLink("create",
                resources.Url<ContentsController>(x => nameof(x.PostContent), values));

            if (resources.CanChangeStatus(values.schema) && await workflow.CanPublishInitialAsync(schema, resources.Context.UserPrincipal))
            {
                var publishValues = new { values.app, values.schema, publish = true };

                AddPostLink("create/publish",
                    resources.Url<ContentsController>(x => nameof(x.PostContent), publishValues));
            }
        }
    }
}
