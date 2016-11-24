// ==========================================================================
//  AppClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Write.Apps
{
    public sealed class AppClient
    {
        public string ClientName { get; }

        public string ClientSecret { get; }

        public DateTime ExpiresUtc { get; }

        public AppClient(string name, string secret, DateTime expiresUtc)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNullOrEmpty(secret, nameof(secret));

            ClientName = name;
            ClientSecret = secret;

            ExpiresUtc = expiresUtc;
        }
    }
}
