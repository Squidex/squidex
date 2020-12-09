// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class BulkUpdateDto
    {
        /// <summary>
        /// The contents to update or insert.
        /// </summary>
        [LocalizedRequired]
        public BulkUpdateJobDto[]? Jobs { get; set; }

        /// <summary>
        /// True to automatically publish the content.
        /// </summary>
        public bool Publish { get; set; }

        /// <summary>
        /// True to turn off scripting for faster inserts. Default: true.
        /// </summary>
        public bool DoNotScript { get; set; } = true;

        /// <summary>
        /// True to check referrers of this content.
        /// </summary>
        public bool CheckReferrers { get; set; }

        /// <summary>
        /// True to turn off costly validation: Unique checks, asset checks and reference checks. Default: true.
        /// </summary>
        public bool OptimizeValidation { get; set; } = true;

        public BulkUpdateContents ToCommand()
        {
            var result = SimpleMapper.Map(this, new BulkUpdateContents());

            result.Jobs = Jobs?.Select(x => x.ToJob())?.ToArray();

            return result;
        }
    }
}
