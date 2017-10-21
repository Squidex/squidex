// ==========================================================================
//  AppClient.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppClient
    {
        private readonly string secret;
        private string name;
        private AppClientPermission permission;

        public AppClient(string name, string secret)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNullOrEmpty(secret, nameof(secret));

            this.name = name;

            this.secret = secret;
        }

        public void Update(AppClientPermission newPermission)
        {
            Guard.Enum(newPermission, nameof(newPermission));

            permission = newPermission;
        }

        public void Rename(string newName)
        {
            Guard.NotNullOrEmpty(newName, nameof(newName));

            name = newName;
        }
    }
}
