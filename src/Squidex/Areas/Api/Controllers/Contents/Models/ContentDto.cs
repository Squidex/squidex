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
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ContentDto : IGenerateEtag
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
        /// The the status of the content.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The version of the content.
        /// </summary>
        public long Version { get; set; }

        public static ContentDto FromCommand(CreateContent command, EntityCreatedResult<NamedContentData> result)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var response = new ContentDto
            {
                Id = command.ContentId,
                Data = result.IdOrValue,
                Version = result.Version,
                Created = now,
                CreatedBy = command.Actor,
                LastModified = now,
                LastModifiedBy = command.Actor,
                Status = command.Publish ? Status.Published : Status.Draft
            };

            return response;
        }

        public static ContentDto FromContent(IContentEntity content, QueryContext context)
        {
            var response = SimpleMapper.Map(content, new ContentDto());

            if (context.Flatten)
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

            return response;
        }
    }
}
