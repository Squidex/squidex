// ==========================================================================
//  ErrorDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Modules.Api
{
    public sealed class ErrorDto
    {
        [Required]
        public string Message { get; set; }

        public string[] Details { get; set; }

        public int? StatusCode { get; set; } = 400;
    }
}
