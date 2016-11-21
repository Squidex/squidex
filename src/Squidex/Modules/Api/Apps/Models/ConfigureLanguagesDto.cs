// ==========================================================================
//  ConfigureLanguagesDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Modules.Api.Apps.Models
{
    public class ConfigureLanguagesDto
    {
        public List<Language> Languages { get; set; }
    }
}
