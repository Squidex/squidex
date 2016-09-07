// ==========================================================================
//  ErrorDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace PinkParrot.Modules.Api
{
    public sealed class ErrorDto
    {
        [Required]
        public string Message { get; set; }

        public string[] Details { get; set; }

        public int? StatusCode { get; set; }
    }
}
