// ==========================================================================
//  MyUIOptions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Config
{
    public sealed class MyUIOptions
    {
        public Dictionary<string, string> RegexSuggestions { get; set; }

        public MapOptions Map { get; set; }

        public sealed class MapOptions
        {
            public string Type { get; set; }

            public MapGoogleOptions GoogleMaps { get; set; }
        }

        public sealed class MapGoogleOptions
        {
            public string Key { get; set; }
        }
    }
}
