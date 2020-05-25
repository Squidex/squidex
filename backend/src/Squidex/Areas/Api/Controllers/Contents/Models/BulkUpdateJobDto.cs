// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public class BulkUpdateJobDto
    {
        /// <summary>
        /// An optional query to identify the content to update.
        /// </summary>
        public Query<IJsonValue>? Query { get; set; }

        /// <summary>
        /// An optional id of the content to update.
        /// </summary>
        public DomainId? Id { get; set; }

        /// <summary>
        /// The data of the content when type is set to 'Upsert'.
        /// </summary>
        public NamedContentData? Data { get; set; }

        /// <summary>
        /// The new status when the type is set to 'ChangeStatus'.
        /// </summary>
        public Status? Status { get; set; }

        /// <summary>
        /// The update type.
        /// </summary>
        public BulkUpdateType Type { get; set; }

        public BulkUpdateJob ToJob()
        {
            return SimpleMapper.Map(this, new BulkUpdateJob());
        }
    }
}
