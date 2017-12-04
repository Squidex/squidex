﻿// ==========================================================================
//  AppPatternDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.UI.Models
{
    public sealed class AppPatternDto
    {
        /// <summary>
        /// The name of the suggestion.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The regex pattern.
        /// </summary>
        [Required]
        public string Pattern { get; set; }

        /// <summary>
        /// The regex message.
        /// </summary>
        public string DefaultMessage { get; set; }
    }
}
