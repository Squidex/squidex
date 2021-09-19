// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class BulkUpdateAssetsDto
    {
        /// <summary>
        /// The contents to update or insert.
        /// </summary>
        [LocalizedRequired]
        public BulkUpdateAssetsJobDto[]? Jobs { get; set; }

        /// <summary>
        /// True to check referrers of deleted assets.
        /// </summary>
        public bool CheckReferrers { get; set; }

        /// <summary>
        /// True to turn off costly validation: Folder checks. Default: true.
        /// </summary>
        public bool OptimizeValidation { get; set; } = true;

        /// <summary>
        /// True to turn off scripting for faster inserts. Default: true.
        /// </summary>
        public bool DoNotScript { get; set; } = true;

        public BulkUpdateAssets ToCommand()
        {
            var result = SimpleMapper.Map(this, new BulkUpdateAssets());

            result.Jobs = Jobs?.Select(x => x.ToJob())?.ToArray();

            return result;
        }
    }
}
