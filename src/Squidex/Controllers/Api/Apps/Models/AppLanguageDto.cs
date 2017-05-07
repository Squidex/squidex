// ==========================================================================
//  AppLanguageDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Apps.Models
{
    public class AppLanguageDto
    {
        /// <summary>
        /// The iso code of the language.
        /// </summary>
        [Required]
        public string Iso2Code { get; set; }

        /// <summary>
        /// The english name of the language.
        /// </summary>
        [Required]
        public string EnglishName { get; set; }

        /// <summary>
        /// Indicates if the language is the master language.
        /// </summary>
        public bool IsMaster { get; set; }

        /// <summary>
        /// Indicates if the language is optional.
        /// </summary>
        public bool IsOptional { get; set; }
    }
}
