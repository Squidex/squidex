// ==========================================================================
//  SetMasterLanguageDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.Apps.Models
{
    public class SetMasterLanguageDto
    {
        /// <summary>
        /// Set the value to true to make the language to the master language.
        /// </summary>
        public bool IsMasterLanguage { get; set; }
    }
}
