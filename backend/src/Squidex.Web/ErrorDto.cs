// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Web
{
    public sealed class ErrorDto
    {
        [LocalizedRequired]
        [Display(Description = "Error message.")]
        public string? Message { get; set; }

        [Display(Description = "The error code.")]
        public string? ErrorCode { get; set; }

        [Display(Description = "The optional trace id.")]
        public string? TraceId { get; set; }

        [Display(Description = "Link to the error details.")]
        public string? Type { get; set; }

        [Display(Description = "Detailed error messages.")]
        public string[]? Details { get; set; }

        [Display(Description = "Status code of the http response.")]
        public int StatusCode { get; set; } = 400;
    }
}
