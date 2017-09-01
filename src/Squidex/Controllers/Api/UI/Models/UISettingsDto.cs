// ==========================================================================
//  UISettingsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.UI.Models
{
    public sealed class UISettingsDto
    {
        /// <summary>
        /// The regex suggestions.
        /// </summary>
        [Required]
        public List<UIRegexSuggestionDto> RegexSuggestions { get; set; }
    }
}
