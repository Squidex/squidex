// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Security
{
    public sealed partial class Permission : IComparable<Permission>, IEquatable<Permission>
    {
        public const string Any = "*";
        public const string Exclude = "^";

        private Part[] path;

        public string Id { get; }

        private Part[] Path
        {
            get => path ??= Part.ParsePath(Id);
        }

        public Permission(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            Id = id;
        }

        public bool Allows(Permission permission)
        {
            if (permission == null)
            {
                return false;
            }

            return Covers(Path, permission.Path);
        }

        public bool Includes(Permission permission)
        {
            if (permission == null)
            {
                return false;
            }

            return PartialCovers(Path, permission.Path);
        }

        private static bool Covers(Part[] given, Part[] requested)
        {
            if (given.Length > requested.Length)
            {
                return false;
            }

            for (var i = 0; i < given.Length; i++)
            {
                if (!Part.Intersects(ref given[i], ref requested[i], false))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool PartialCovers(Part[] given, Part[] requested)
        {
            for (var i = 0; i < Math.Min(given.Length, requested.Length); i++)
            {
                if (!Part.Intersects(ref given[i], ref requested[i], true))
                {
                    return false;
                }
            }

            return true;
        }

        public bool StartsWith(string test)
        {
            return Id.StartsWith(test, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Permission);
        }

        public bool Equals(Permission? other)
        {
            return other != null && other.Id.Equals(Id, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode(StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return Id;
        }

        public int CompareTo(Permission? other)
        {
            return other == null ? -1 : string.Compare(Id, other.Id, StringComparison.Ordinal);
        }
    }
}
