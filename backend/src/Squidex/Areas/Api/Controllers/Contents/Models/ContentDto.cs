// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Areas.Api.Controllers.Schemas.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

public sealed class ContentDto : Resource
{
    /// <summary>
    /// The if of the content item.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The user that has created the content item.
    /// </summary>
    [LocalizedRequired]
    public RefToken CreatedBy { get; set; }

    /// <summary>
    /// The user that has updated the content item.
    /// </summary>
    [LocalizedRequired]
    public RefToken LastModifiedBy { get; set; }

    /// <summary>
    /// The data of the content item.
    /// </summary>
    [LocalizedRequired]
    public object Data { get; set; }

    /// <summary>
    /// The reference data for the frontend UI.
    /// </summary>
    public ContentData? ReferenceData { get; set; }

    /// <summary>
    /// The date and time when the content item has been created.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The date and time when the content item has been modified last.
    /// </summary>
    public Instant LastModified { get; set; }

    /// <summary>
    /// The status of the content.
    /// </summary>
    public Status Status { get; set; }

    /// <summary>
    /// The new status of the content.
    /// </summary>
    public Status? NewStatus { get; set; }

    /// <summary>
    /// The color of the status.
    /// </summary>
    public string StatusColor { get; set; }

    /// <summary>
    /// The color of the new status.
    /// </summary>
    public string? NewStatusColor { get; set; }

    /// <summary>
    /// The UI token.
    /// </summary>
    public string? EditToken { get; set; }

    /// <summary>
    /// The scheduled status.
    /// </summary>
    public ScheduleJobDto? ScheduleJob { get; set; }

    /// <summary>
    /// The ID of the schema.
    /// </summary>
    public DomainId SchemaId { get; set; }

    /// <summary>
    /// The name of the schema.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// The display name of the schema.
    /// </summary>
    public string? SchemaDisplayName { get; set; }

    /// <summary>
    /// The reference fields.
    /// </summary>
    public FieldDto[]? ReferenceFields { get; set; }

    /// <summary>
    /// Indicates whether the content is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The version of the content.
    /// </summary>
    public long Version { get; set; }

    public static ContentDto FromDomain(IEnrichedContentEntity content, Resources resources)
    {
        var response = SimpleMapper.Map(content, new ContentDto
        {
            SchemaId = content.SchemaId.Id,
            SchemaName = content.SchemaId.Name
        });

        if (resources.Context.ShouldFlatten())
        {
            response.Data = content.Data.ToFlatten();
        }
        else
        {
            response.Data = content.Data;
        }

        if (content.ReferenceFields != null)
        {
            response.ReferenceFields = content.ReferenceFields.Select(FieldDto.FromDomain).ToArray();
        }

        if (content.ScheduleJob != null)
        {
            response.ScheduleJob = new ScheduleJobDto
            {
                Color = content.ScheduledStatusColor!
            };

            SimpleMapper.Map(content.ScheduleJob, response.ScheduleJob);
        }

        if (response.IsDeleted)
        {
            return response;
        }

        return response.CreateLinksAsync(content, resources, content.SchemaId.Name);
    }

    private ContentDto CreateLinksAsync(IEnrichedContentEntity content, Resources resources, string schema)
    {
        var app = resources.App;

        var values = new { app, schema, id = Id };

        AddSelfLink(resources.Url<ContentsController>(x => nameof(x.GetContent), values));

        if (Version > 0)
        {
            var versioned = new { app, schema, values.id, version = Version - 1 };

            AddGetLink("previous",
                resources.Url<ContentsController>(x => nameof(x.GetContentVersion), versioned));
        }

        if (NewStatus != null)
        {
            if (resources.CanDeleteContentVersion(schema))
            {
                AddDeleteLink("draft/delete",
                    resources.Url<ContentsController>(x => nameof(x.DeleteVersion), values));
            }
        }
        else if (Status == Status.Published)
        {
            if (resources.CanCreateContentVersion(schema))
            {
                AddPostLink("draft/create",
                    resources.Url<ContentsController>(x => nameof(x.CreateDraft), values));
            }
        }

        if (content.NextStatuses != null && resources.CanChangeStatus(schema))
        {
            foreach (var next in content.NextStatuses)
            {
                AddPutLink($"status/{next.Status}", resources.Url<ContentsController>(x => nameof(x.PutContentStatus), values), next.Color);
            }
        }

        if (content.ScheduleJob != null && resources.CanCancelContentStatus(schema))
        {
            AddDeleteLink($"cancel", resources.Url<ContentsController>(x => nameof(x.DeleteContentStatus), values));
        }

        if (!content.IsSingleton && resources.CanDeleteContent(schema))
        {
            AddDeleteLink("delete",
                resources.Url<ContentsController>(x => nameof(x.DeleteContent), values));
        }

        if (content.CanUpdate && resources.CanUpdateContent(schema))
        {
            AddPatchLink("patch",
                resources.Url<ContentsController>(x => nameof(x.PatchContent), values));
        }

        if (content.CanUpdate && resources.CanUpdateContent(schema))
        {
            AddPutLink("update",
                resources.Url<ContentsController>(x => nameof(x.PutContent), values));
        }

        return this;
    }
}
