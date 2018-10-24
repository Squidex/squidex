// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Security
{
    public sealed class Permission : IComparable<Permission>, IEquatable<Permission>
    {
        private const string Any = "*";
        private static readonly char[] MainSeparators = { '.' };
        private static readonly char[] AlternativeSeparators = { '|' };
        private readonly string description;
        private readonly string id;
        private readonly HashSet<string>[] idParts;

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

            idParts = id
                .Split(MainSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    if (x == Any)
                    {
                        return null;
                    }

                    var alternatives = x.Split(AlternativeSeparators, StringSplitOptions.RemoveEmptyEntries);

                    return new HashSet<string>(alternatives, StringComparer.OrdinalIgnoreCase);
                })
                .ToArray();
        }

        public bool Allows(Permission permission)
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

                if (lhs != null && (rhs == null || !lhs.Intersect(rhs).Any()))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Includes(Permission permission)
        {
            if (permission == null)
            {
                return false;
            }

            for (var i = 0; i < Math.Min(idParts.Length, permission.idParts.Length); i++)
            {
                var lhs = idParts[i];
                var rhs = permission.idParts[i];

                if (lhs != null && rhs != null && !lhs.Intersect(rhs).Any())
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
