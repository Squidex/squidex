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
        private readonly string name;
        private readonly AppClientPermission permission;

        public string Name
        {
            get { return name; }
        }

        public string Secret
        {
            get { return secret; }
        }

        public AppClientPermission Permission
        {
            get { return permission; }
        }

        public AppClient(string name, string secret, AppClientPermission permission)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNullOrEmpty(secret, nameof(secret));
            Guard.Enum(permission, nameof(permission));

            this.name = name;
            this.secret = secret;
            this.permission = permission;
        }

        public AppClient Update(AppClientPermission newPermission)
        {
            Guard.Enum(newPermission, nameof(newPermission));

            return new AppClient(name, secret, newPermission);
        }

        public AppClient Rename(string newName)
        {
            Guard.NotNullOrEmpty(newName, nameof(newName));

            return new AppClient(newName, secret, permission);
        }
    }
}
