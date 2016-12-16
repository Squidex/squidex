// ==========================================================================
//  MyEventStoreOptions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Config.EventStore
{
    public sealed class MyEventStoreOptions
    {
        public string IPAddress { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Prefix { get; set; }

        public int Port { get; set; }
    }
}
