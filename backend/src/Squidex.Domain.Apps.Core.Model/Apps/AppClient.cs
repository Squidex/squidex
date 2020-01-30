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
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class AppClient : Named
    {
        public string Role { get; }

        public string Secret { get; }

        public AppClient(string name, string secret, string role)
            : base(name)
        {
            Guard.NotNullOrEmpty(secret);
            Guard.NotNullOrEmpty(role);

            Role = role;

            Secret = secret;
        }

        [Pure]
        public AppClient Update(string newRole)
        {
            Guard.NotNullOrEmpty(newRole);

            return new AppClient(Name, Secret, newRole);
        }

        [Pure]
        public AppClient Rename(string newName)
        {
            Guard.NotNullOrEmpty(newName);

            return new AppClient(newName, Secret, Role);
        }
    }
}
