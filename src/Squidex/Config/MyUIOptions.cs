// ==========================================================================
//  MyUIOptions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Areas.Api.Controllers.Apps.Models;

namespace Squidex.Config
{
    public sealed class MyUIOptions
    {
        public List<AppPatternDto> RegexSuggestions { get; set; }
    }
}
