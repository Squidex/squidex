// ==========================================================================
//  UpdateAppLanguageDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Controllers.Api.Apps.Models
{
    public sealed class UpdateAppLanguageDto
    {
        /// <summary>
        /// Set the value to true to make the language the master.
        /// </summary>
        public bool? IsMaster { get; set; }

        /// <summary>
        /// Set the value to true to make the language optional.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Optional fallback languages.
        /// </summary>
        public List<Language> Fallback { get; set; }
    }
}
