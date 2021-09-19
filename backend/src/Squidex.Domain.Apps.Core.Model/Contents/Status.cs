// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel;

namespace Squidex.Domain.Apps.Core.Contents
{
    [TypeConverter(typeof(StatusTypeConverter))]
    public readonly struct Status : IEquatable<Status>, IComparable<Status>
    {
        public static readonly Status Archived = new Status("Archived");
        public static readonly Status Draft = new Status("Draft");
        public static readonly Status Published = new Status("Published");

        private readonly string? name;

        public string Name
        {
            get => name ?? "Unknown";
        }

        public Status(string? name)
        {
            this.name = name;
        }

        public override bool Equals(object? obj)
        {
            return obj is Status status && Equals(status);
        }

        public bool Equals(Status other)
        {
            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode(StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(Status other)
        {
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public static bool operator ==(Status lhs, Status rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Status lhs, Status rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
