// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using StatusType = Squidex.Domain.Apps.Core.Contents.Status;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public class UpsertContentDto
    {
        /// <summary>
        /// The full data for the content item.
        /// </summary>
        [FromBody]
        public ContentData Data { get; set; }

        /// <summary>
        /// The initial status.
        /// </summary>
        [FromQuery]
        public StatusType? Status { get; set; }

        /// <summary>
        /// True to automatically publish the content.
        /// </summary>
        [FromQuery]
        [Obsolete("Use 'status' query string now.")]
        public bool Publish { get; set; }

        public UpsertContent ToCommand(DomainId id)
        {
            var command = new UpsertContent { Data = Data!, ContentId = id };

            if (Status != null)
            {
                command.Status = Status;
            }
#pragma warning disable CS0618 // Type or member is obsolete
            else if (Publish)
            {
                command.Status = StatusType.Published;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return command;
        }
    }
}
