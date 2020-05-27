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
        public static readonly DomainId Empty = default;
        public static readonly DomainId EmptyGuid = new DomainId(Guid.Empty.ToString());

        private readonly string? id;

        public DomainId(Guid id)
        {
            this.id = id.ToString();
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
            return string.Equals(ToString(), other.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return id ?? "<EMPTY>";
        }

        public int CompareTo([AllowNull] DomainId other)
        {
            return string.Compare(id, other.id, StringComparison.Ordinal);
        }

        public static implicit operator DomainId(string value)
        {
            if (value == null)
            {
                return Empty;
            }

            return new DomainId(value);
        }

        public static implicit operator DomainId(Guid value)
        {
            if (value == Guid.Empty)
            {
                return EmptyGuid;
            }

            return new DomainId(value);
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
