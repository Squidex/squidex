// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class BulkUpdateContentsDto
    {
        /// <summary>
        /// The contents to update or insert.
        /// </summary>
        [LocalizedRequired]
        public BulkUpdateContentsJobDto[]? Jobs { get; set; }

        /// <summary>
        /// True to automatically publish the content.
        /// </summary>
        [Obsolete("Use Jobs.Status")]
        public bool Publish { get; set; }

        /// <summary>
        /// True to turn off scripting for faster inserts. Default: true.
        /// </summary>
        public bool DoNotScript { get; set; } = true;

        /// <summary>
        /// True to turn off validation for faster inserts. Default: false.
        /// </summary>
        public bool DoNotValidate { get; set; }

        /// <summary>
        /// True to turn off validation of workflow rules. Default: false.
        /// </summary>
        public bool DoNotValidateWorkflow { get; set; }

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

            if (result.Jobs != null && Publish)
            {
                foreach (var job in result.Jobs)
                {
                    if (job != null)
                    {
                        job.Status = Status.Published;
                    }
                }
            }

            return result;
        }
    }
}
