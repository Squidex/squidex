// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public class BulkUpdateContentsJobDto
    {
        /// <summary>
        /// An optional query to identify the content to update.
        /// </summary>
        public JsonQueryDto? Query { get; set; }

        /// <summary>
        /// An optional id of the content to update.
        /// </summary>
        public DomainId? Id { get; set; }

        /// <summary>
        /// The data of the content when type is set to 'Upsert', 'Create', 'Update' or 'Patch.
        /// </summary>
        public ContentData? Data { get; set; }

        /// <summary>
        /// The new status when the type is set to 'ChangeStatus' or 'Upsert'.
        /// </summary>
        public Status? Status { get; set; }

        /// <summary>
        /// The due time.
        /// </summary>
        public Instant? DueTime { get; set; }

        /// <summary>
        /// The update type.
        /// </summary>
        public BulkUpdateContentType Type { get; set; }

        /// <summary>
        /// The optional schema id or name.
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// True to delete the content permanently.
        /// </summary>
        public bool Permanent { get; set; }

        /// <summary>
        /// The number of expected items. Set it to a higher number to update multiple items when a query is defined.
        /// </summary>
        public long ExpectedCount { get; set; } = 1;

        /// <summary>
        /// The expected version.
        /// </summary>
        public long ExpectedVersion { get; set; } = EtagVersion.Any;

        public BulkUpdateJob ToJob()
        {
            return SimpleMapper.Map(this, new BulkUpdateJob());
        }
    }
}
