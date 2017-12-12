// ==========================================================================
//  UISettingsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.UI.Models
{
    public sealed class UISettingsDto
    {
        /// <summary>
        /// The regex suggestions.
        /// </summary>
        [Required]
        public List<UIRegexSuggestionDto> RegexSuggestions { get; set; }

        /// <summary>
        /// The geocoder key.
        /// </summary>
        [Required]
        public string GeocoderKey { get; set; }
    }
}
