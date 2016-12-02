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
        private string name;

        public string ClientId { get; }

        public string ClientSecret { get; }

        public DateTime ExpiresUtc { get; }

        public string Name
        {
            get { return name ?? ClientId; }
        }

        public AppClient(string id, string secret, DateTime expiresUtc)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNullOrEmpty(secret, nameof(secret));

            ClientId = id;
            ClientSecret = secret;

            ExpiresUtc = expiresUtc;
        }

        public void Rename(string newName)
        {
            name = newName;
        }
    }
}
