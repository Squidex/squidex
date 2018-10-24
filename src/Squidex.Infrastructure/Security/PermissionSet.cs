// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Security
{
    public sealed class PermissionSet : IReadOnlyCollection<Permission>
    {
        public static readonly PermissionSet Empty = new PermissionSet();

        private readonly List<Permission> permissions;

        public int Count
        {
            get { return permissions.Count; }
        }

        public PermissionSet(IEnumerable<Permission> permissions)
        {
            Guard.NotNull(permissions, nameof(permissions));

            this.permissions = permissions.ToList();
        }

        public PermissionSet(params Permission[] permissions)
        {
            Guard.NotNull(permissions, nameof(permissions));

            this.permissions = permissions.ToList();
        }

        public bool GivesPermissionTo(Permission other)
        {
            if (other == null)
            {
                return false;
            }

            foreach (var permission in permissions)
            {
                if (permission.Allows(other))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<Permission> GetEnumerator()
        {
            return permissions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return permissions.GetEnumerator();
        }
    }
}
