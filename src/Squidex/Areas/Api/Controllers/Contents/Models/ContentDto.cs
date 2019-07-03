// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
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
        /// The pending changes of the content item.
        /// </summary>
        public object DataDraft { get; set; }

        /// <summary>
        /// Indicates if the draft data is pending.
        /// </summary>
        public bool IsPending { get; set; }

        /// <summary>
        /// The scheduled status.
        /// </summary>
        public ScheduleJobDto ScheduleJob { get; set; }

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
        /// The color of the status.
        /// </summary>
        public string StatusColor { get; set; }

        /// <summary>
        /// The version of the content.
        /// </summary>
        public long Version { get; set; }

        public static ContentDto FromContent(Context context, IEnrichedContentEntity content, ApiController controller)
        {
            var response = SimpleMapper.Map(content, new ContentDto());

            if (context.IsFlatten())
            {
                response.Data = content.Data?.ToFlatten();
                response.DataDraft = content.DataDraft?.ToFlatten();
            }
            else
            {
                response.Data = content.Data;
                response.DataDraft = content.DataDraft;
            }

            if (content.ScheduleJob != null)
            {
                response.ScheduleJob = SimpleMapper.Map(content.ScheduleJob, new ScheduleJobDto());
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

                AddGetLink("prev", controller.Url<ContentsController>(x => nameof(x.GetContentVersion), versioned));
            }

            if (IsPending)
            {
                if (controller.HasPermission(Permissions.AppContentsDraftDiscard, app, schema))
                {
                    AddPutLink("draft/discard", controller.Url<ContentsController>(x => nameof(x.DiscardDraft), values));
                }

                if (controller.HasPermission(Permissions.AppContentsDraftPublish, app, schema))
                {
                    AddPutLink("draft/publish", controller.Url<ContentsController>(x => nameof(x.PutContentStatus), values));
                }
            }

            if (controller.HasPermission(Permissions.AppContentsUpdate, app, schema))
            {
                if (content.CanUpdate)
                {
                    AddPutLink("update", controller.Url<ContentsController>(x => nameof(x.PutContent), values));
                }

                if (Status == Status.Published)
                {
                    AddPutLink("draft/propose", controller.Url((ContentsController x) => nameof(x.PutContent), values) + "?asDraft=true");
                }

                AddPatchLink("patch", controller.Url<ContentsController>(x => nameof(x.PatchContent), values));

                if (content.Nexts != null)
                {
                    foreach (var next in content.Nexts)
                    {
                        AddPutLink($"status/{next.Status}", controller.Url<ContentsController>(x => nameof(x.PutContentStatus), values), next.Color);
                    }
                }
            }

            if (controller.HasPermission(Permissions.AppContentsDelete, app, schema))
            {
                AddDeleteLink("delete", controller.Url<ContentsController>(x => nameof(x.DeleteContent), values));
            }

            return this;
        }
    }
}
