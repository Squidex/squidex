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
        public const string Any = "*";

        private static readonly char[] MainSeparators = { '.' };
        private static readonly char[] AlternativeSeparators = { '|' };
        private readonly string id;
        private readonly Lazy<HashSet<string>[]> idParts;

        public string Id
        {
            get { return id; }
        }

        public Permission(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            this.id = id;

            idParts = new Lazy<HashSet<string>[]>(() => id
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
                .ToArray());
        }

        public bool Allows(Permission permission)
        {
            if (permission == null)
            {
                return false;
            }

            var lhs = idParts.Value;
            var rhs = permission.idParts.Value;

            if (lhs.Length > rhs.Length)
            {
                return false;
            }

            for (var i = 0; i < lhs.Length; i++)
            {
                var l = lhs[i];
                var r = rhs[i];

                if (l != null && (r == null || !l.Intersect(r).Any()))
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

            var lhs = idParts.Value;
            var rhs = permission.idParts.Value;

            for (var i = 0; i < Math.Min(lhs.Length, rhs.Length); i++)
            {
                var l = lhs[i];
                var r = rhs[i];

                if (l != null && r != null && !l.Intersect(r).Any())
                {
                    return false;
                }
            }

            return true;
        }

        public bool StartsWith(string id)
        {
            return id.StartsWith(id, StringComparison.OrdinalIgnoreCase);
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
