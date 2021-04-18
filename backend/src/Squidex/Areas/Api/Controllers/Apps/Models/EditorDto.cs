// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class EditorDto
    {
        /// <summary>
        /// The name of the editor.
        /// </summary>
        [LocalizedRequired]
        public string Name { get; set; }

        /// <summary>
        /// The url to the editor.
        /// </summary>
        [LocalizedRequired]
        public string Url { get; set; }
    }
}
