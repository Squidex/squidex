// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NodaTime;
using Squidex.Areas.Api.Controllers.Schemas.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ContentDto : Resource
    {
        /// <summary>
        /// The if of the content item.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The user that has created the content item.
        /// </summary>
        [Required]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the content item.
        /// </summary>
        [Required]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The data of the content item.
        /// </summary>
        [Required]
        public object Data { get; set; }

        /// <summary>
        /// The reference data for the frontend UI.
        /// </summary>
        public NamedContentData? ReferenceData { get; set; }

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
        /// The scheduled status.
        /// </summary>
        public ScheduleJobDto? ScheduleJob { get; set; }

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
        /// The version of the content.
        /// </summary>
        public long Version { get; set; }

        public static ContentDto FromContent(Context context, IEnrichedContentEntity content, ApiController controller)
        {
            var response = SimpleMapper.Map(content, new ContentDto());

            if (context.ShouldFlatten())
            {
                response.Data = content.Data.ToFlatten();
            }
            else
            {
                response.Data = content.Data;
            }

            if (content.ReferenceFields != null)
            {
                response.ReferenceFields = content.ReferenceFields.Select(FieldDto.FromField).ToArray();
            }

            if (content.ScheduleJob != null)
            {
                response.ScheduleJob = new ScheduleJobDto
                {
                    Color = content.ScheduledStatusColor!
                };

                SimpleMapper.Map(content.ScheduleJob, response.ScheduleJob);
            }

            return response.CreateLinksAsync(content, controller, content.AppId.Name, content.SchemaId.Name);
        }

        private ContentDto CreateLinksAsync(IEnrichedContentEntity content, ApiController controller, string app, string schema)
        {
            var values = new { app, name = schema, id = Id };

            AddSelfLink(controller.Url<ContentsController>(x => nameof(x.GetContent), values));

            if (Version > 0)
            {
                var versioned = new { app, name = schema, id = Id, version = Version - 1 };

                AddGetLink("previous", controller.Url<ContentsController>(x => nameof(x.GetContentVersion), versioned));
            }

            if (!content.IsSingleton)
            {
                if (NewStatus.HasValue)
                {
                    if (controller.HasPermission(Permissions.AppContentsVersionDelete, app, schema))
                    {
                        AddPutLink("draft/delete", controller.Url<ContentsController>(x => nameof(x.DeleteVersion), values));
                    }
                }
                else if (Status == Status.Published)
                {
                    if (controller.HasPermission(Permissions.AppContentsVersionCreate, app, schema))
                    {
                        AddPutLink("draft/create", controller.Url<ContentsController>(x => nameof(x.CreateDraft), values));
                    }
                }

                if (content.NextStatuses != null)
                {
                    foreach (var next in content.NextStatuses)
                    {
                        AddPutLink($"status/{next.Status}", controller.Url<ContentsController>(x => nameof(x.PutContentStatus), values), next.Color);
                    }
                }

                if (controller.HasPermission(Permissions.AppContentsDelete, app, schema))
                {
                    AddDeleteLink("delete", controller.Url<ContentsController>(x => nameof(x.DeleteContent), values));
                }
            }

            if (content.CanUpdate)
            {
                if (controller.HasPermission(Permissions.AppContentsUpdate, app, schema))
                {
                    AddPutLink("update", controller.Url<ContentsController>(x => nameof(x.PutContent), values));
                }

                if (controller.HasPermission(Permissions.AppContentsUpdatePartial, app, schema))
                {
                    AddPatchLink("patch", controller.Url<ContentsController>(x => nameof(x.PatchContent), values));
                }
            }

            return this;
        }
    }
}
