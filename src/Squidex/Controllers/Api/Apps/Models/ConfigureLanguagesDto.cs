// ==========================================================================
//  ConfigureLanguagesDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Controllers.Api.Apps.Models
{
    public sealed class ConfigureLanguagesDto
    {
        /// <summary>
        /// The list of languages to configure the app.
        /// </summary>
        public List<Language> Languages { get; set; }
    }
}
