// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure
{
    [TypeConverter(typeof(DomainIdTypeConverter))]
    public readonly struct DomainId : IEquatable<DomainId>, IComparable<DomainId>
    {
        private static readonly string EmptyString = Guid.Empty.ToString();
        public static readonly DomainId Empty = default;

        private readonly string? id;

        private DomainId(string id)
        {
            this.id = id;
        }

        public static DomainId? CreateNullable(string? value)
        {
            if (value == null)
            {
                return null;
            }

            return new DomainId(value);
        }

        public static DomainId Create(string value)
        {
            if (value == null || string.Equals(value, EmptyString, StringComparison.OrdinalIgnoreCase))
            {
                return Empty;
            }

            return new DomainId(value);
        }

        public static DomainId Create(Guid value)
        {
            if (value == Guid.Empty)
            {
                return Empty;
            }

            return new DomainId(value.ToString());
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
            return id ?? EmptyString;
        }

        public int CompareTo([AllowNull] DomainId other)
        {
            return string.Compare(id, other.id, StringComparison.Ordinal);
        }

        public static implicit operator DomainId(string value)
        {
            return Create(value);
        }

        public static implicit operator DomainId(Guid value)
        {
            return Create(value);
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
            return new DomainId($"{id1.Id}--{id2}");
        }

        public static DomainId Combine(DomainId id1, DomainId id2)
        {
            return new DomainId($"{id1}--{id2}");
        }
    }
}
