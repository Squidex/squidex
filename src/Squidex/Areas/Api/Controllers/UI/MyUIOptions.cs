// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Areas.Api.Controllers.UI
{
    public sealed class MyUIOptions
    {
        public Dictionary<string, string> RegexSuggestions { get; set; }

        public Dictionary<string, string> More { get; set; } = new Dictionary<string, string>();

        public MapOptions Map { get; set; }

        public bool ShowInfo { get; set; }

        public bool HideNews { get; set; }

        public bool HideOnboarding { get; set; }

        public bool RedirectToLogin { get; set; }

        public bool OnlyAdminsCanCreateApps { get; set; }

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
