// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel;

namespace Squidex.Infrastructure
{
    [TypeConverter(typeof(DomainIdTypeConverter))]
    public readonly struct DomainId : IEquatable<DomainId>, IComparable<DomainId>
    {
        private static readonly string EmptyString = Guid.Empty.ToString();
        public static readonly DomainId Empty = default;
        public static readonly string IdSeparator = "--";

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

            return Create(value);
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
            return string.Equals(ToString(), other.ToString(), StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode(StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return id ?? EmptyString;
        }

        public int CompareTo(DomainId other)
        {
            return string.Compare(ToString(), other.ToString(), StringComparison.Ordinal);
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
            return new DomainId($"{id1.Id}{IdSeparator}{id2}");
        }

        public static DomainId Combine(DomainId id1, DomainId id2)
        {
            return new DomainId($"{id1}{IdSeparator}{id2}");
        }
    }
}
