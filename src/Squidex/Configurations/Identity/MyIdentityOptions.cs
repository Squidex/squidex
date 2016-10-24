// ==========================================================================
//  MyIdentityOptions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Configurations.Identity
{
    public sealed class MyIdentityOptions
    {
        public string DefaultUsername { get; set; }

        public string DefaultPassword { get; set; }

        public string GoogleClient { get; set; }

        public string GoogleSecret { get; set; }

        public string BaseUrl { get; set; }

        public bool RequiresHttps { get; set; }
    }
}
