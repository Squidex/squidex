// ==========================================================================
//  EventStoreOptions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

namespace PinkParrot.Configurations
{
    public sealed class EventStoreOptions
    {
        public string IPAddress { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Prefix { get; set; }

        public int Port { get; set; }
    }
}
