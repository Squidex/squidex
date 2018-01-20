// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Pipeline
{
    public sealed class ErrorDto
    {
        /// <summary>
        /// Error message.
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// Detailed error messages.
        /// </summary>
        public string[] Details { get; set; }

        /// <summary>
        /// Status code of the http response.
        /// </summary>
        public int? StatusCode { get; set; } = 400;
    }
}
