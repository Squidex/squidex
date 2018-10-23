// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Security
{
    public sealed class Permission : IComparable<Permission>, IEquatable<Permission>
    {
        private const string Any = "*";
        private static readonly char[] Separators = { '.' };
        private readonly string description;
        private readonly string id;
        private readonly string[] idParts;

        public string Id
        {
            get { return id; }
        }

        public string Description
        {
            get { return description; }
        }

        public Permission(string id, string description = null)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            this.description = description;

            this.id = id;
            this.idParts = id.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool GivesPermissionTo(Permission permission)
        {
            if (permission == null)
            {
                return false;
            }

            if (idParts.Length > permission.idParts.Length)
            {
                return false;
            }

            for (var i = 0; i < idParts.Length; i++)
            {
                var lhs = idParts[i];
                var rhs = permission.idParts[i];

                if (!string.Equals(lhs, Any, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(lhs, rhs, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Permission);
        }

        public bool Equals(Permission other)
        {
            return other != null && string.Equals(id, other.id, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return id;
        }

        public int CompareTo(Permission other)
        {
            return other == null ? -1 : string.Compare(id, other.id, StringComparison.Ordinal);
        }
    }
}
