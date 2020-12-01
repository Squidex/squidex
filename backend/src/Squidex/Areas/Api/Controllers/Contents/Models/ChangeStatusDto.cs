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
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ChangeStatusDto
    {
        /// <summary>
        /// The new status.
        /// </summary>
        [LocalizedRequired]
        public Status Status { get; set; }

        /// <summary>
        /// The due time.
        /// </summary>
        public Instant? DueTime { get; set; }

        /// <summary>
        /// True to check referrers of this content.
        /// </summary>
        public bool CheckReferrers { get; set; }

        public ChangeContentStatus ToCommand(DomainId id)
        {
            return SimpleMapper.Map(this, new ChangeContentStatus { ContentId = id });
        }
    }
}
