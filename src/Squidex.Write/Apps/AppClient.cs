// ==========================================================================
//  AppClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Write.Apps
{
    public sealed class AppClient
    {
        private readonly string name;
        private readonly string id;
        private readonly string secret;

        public string Id
        {
            get { return id; }
        }

        public string Name
        {
            get { return name ?? Id; }
        }

        public string Secret
        {
            get { return secret; }
        }

        public AppClient(string id, string secret, string name = null)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNullOrEmpty(secret, nameof(secret));

            this.id = id;
            this.name = name;
            this.secret = secret;
        }

        public AppClient Rename(string newName)
        {
            return new AppClient(Id, Secret, newName);
        }
    }
}
