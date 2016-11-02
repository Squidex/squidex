// ==========================================================================
//  MyIdentityOptions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Configurations.Identity
{
    public sealed class MyIdentityOptions
    {
        public string BaseUrl { get; set; }

        public string DefaultUsername { get; set; }

        public string DefaultPassword { get; set; }

        public string GoogleClient { get; set; }

        public string GoogleSecret { get; set; }

        public bool RequiresHttps { get; set; }

        public string BuildUrl(string path)
        {
            var url = $"{BaseUrl.TrimEnd('/')}/{path.Trim('/')}";

            if (url.IndexOf("?", StringComparison.OrdinalIgnoreCase) < 0 &&
                url.IndexOf(";", StringComparison.OrdinalIgnoreCase) < 0) {
                url = url + "/";
            }

            return url;
        }
    }
}
