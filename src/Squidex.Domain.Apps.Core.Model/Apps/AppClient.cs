// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppClient
    {
        private readonly string name;
        private readonly string secret;
        private readonly string role;

        public string Name
        {
            get { return name; }
        }

        public string Secret
        {
            get { return secret; }
        }

        public string Role
        {
            get { return role; }
        }

        public AppClient(string name, string secret, string role)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNullOrEmpty(secret, nameof(secret));
            Guard.NotNullOrEmpty(role, nameof(role));

            this.name = name;
            this.role = role;
            this.secret = secret;
        }

        [Pure]
        public AppClient Update(string newRole)
        {
            Guard.NotNullOrEmpty(newRole, nameof(newRole));

            return new AppClient(name, secret, newRole);
        }

        [Pure]
        public AppClient Rename(string newName)
        {
            Guard.NotNullOrEmpty(newName, nameof(newName));

            return new AppClient(newName, secret, role);
        }
    }
}
