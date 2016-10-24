// ==========================================================================
//  MyIdentityOptions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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
            return $"{BaseUrl.TrimEnd('/')}/{path.Trim('/')}/";
        }
    }
}
