// ==========================================================================
//  ErrorDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
