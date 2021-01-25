// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ImportContentsDto
    {
        /// <summary>
        /// The data to import.
        /// </summary>
        [LocalizedRequired]
        public List<ContentData> Datas { get; set; }

        /// <summary>
        /// True to automatically publish the content.
        /// </summary>
        public bool Publish { get; set; }

        /// <summary>
        /// True to turn off scripting for faster inserts. Default: true.
        /// </summary>
        public bool DoNotScript { get; set; } = true;

        /// <summary>
        /// True to turn off costly validation: Unique checks, asset checks and reference checks. Default: true.
        /// </summary>
        public bool OptimizeValidation { get; set; } = true;

        public BulkUpdateContents ToCommand()
        {
            return SimpleMapper.Map(this, new BulkUpdateContents());
        }
    }
}
