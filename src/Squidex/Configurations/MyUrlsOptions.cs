// ==========================================================================
//  MyUrlsOptions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Configurations
{
    public sealed class MyUrlsOptions
    {
        public string BaseUrl { get; set; }

        public string BuildUrl(string path, bool trailingSlash = true)
        {
            var url = $"{BaseUrl.TrimEnd('/')}/{path.Trim('/')}";

            if (trailingSlash &&
                url.IndexOf("?", StringComparison.OrdinalIgnoreCase) < 0 &&
                url.IndexOf(";", StringComparison.OrdinalIgnoreCase) < 0)
            {
                url = url + "/";
            }

            return url;
        }
    }
}
