// ==========================================================================
//  AddAppLanguageDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure;

namespace Squidex.Controllers.Api.Apps.Models
{
    public sealed class AddAppLanguageDto
    {
        /// <summary>
        /// The language to add.
        /// </summary>
        [Required]
        public Language Language { get; set; }
    }
}
