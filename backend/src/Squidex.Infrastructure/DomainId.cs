// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure
{
    public struct DomainId : IEquatable<DomainId>, IComparable<DomainId>
    {
        public static readonly DomainId Empty = default;

        private readonly string? id;

        public bool IsEmpty
        {
            get { return id == null; }
        }

        public string Id
        {
            get { return id ?? "0"; }
        }

        public DomainId(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            this.id = id;
        }

        public override bool Equals(object? obj)
        {
            return obj is DomainId status && Equals(status);
        }

        public bool Equals(DomainId other)
        {
            return string.Equals(id, other.id);
        }

        public override int GetHashCode()
        {
            return id?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Id;
        }

        public int CompareTo([AllowNull] DomainId other)
        {
            return string.Compare(id, other.id, StringComparison.Ordinal);
        }

        public static implicit operator DomainId(string value)
        {
            return new DomainId(value);
        }

        public static implicit operator DomainId(Guid value)
        {
            return new DomainId(value.ToString());
        }

        public static bool operator ==(DomainId lhs, DomainId rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(DomainId lhs, DomainId rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static DomainId NewGuid()
        {
            return new DomainId(Guid.NewGuid().ToString());
        }

        public static DomainId Combine(NamedId<DomainId> id1, DomainId id2)
        {
            return new DomainId($"{id1}-{id2}");
        }

        public static DomainId Combine(DomainId id1, DomainId id2)
        {
            return new DomainId($"{id1}-{id2}");
        }
    }
}
