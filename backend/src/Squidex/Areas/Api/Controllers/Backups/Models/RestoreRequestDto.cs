// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Backups.Models
{
    public sealed class RestoreRequestDto
    {
        /// <summary>
        /// The name of the app.
        /// </summary>
        [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string? Name { get; set; }

        /// <summary>
        /// The url to the restore file.
        /// </summary>
        [LocalizedRequired]
        public Uri Url { get; set; }
    }
}
