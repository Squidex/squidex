﻿// ==========================================================================
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

        private readonly string id;
        private readonly Lazy<Part[]> idParts;

        public string Id
        {
            get { return id; }
        }

        public Permission(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            this.id = id;

            idParts = new Lazy<Part[]>(() => Part.ParsePath(id));
        }

        public bool Allows(Permission permission)
        {
            if (permission == null)
            {
                return false;
            }

            return Covers(idParts.Value, permission.idParts.Value);
        }

        public bool Includes(Permission permission)
        {
            if (permission == null)
            {
                return false;
            }

            return PartialCovers(idParts.Value, permission.idParts.Value);
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
            return id.StartsWith(test, StringComparison.OrdinalIgnoreCase);
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
