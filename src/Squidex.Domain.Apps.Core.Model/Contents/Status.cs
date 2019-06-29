// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel;

namespace Squidex.Domain.Apps.Core.Contents
{
    [TypeConverter(typeof(StatusConverter))]
    public struct Status : IEquatable<Status>
    {
        public static readonly Status Archived = new Status("Archived");
        public static readonly Status Draft = new Status("Draft");
        public static readonly Status Published = new Status("Published");

        private readonly string name;

        public string Name
        {
            get { return name ?? "Unknown"; }
        }

        public Status(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            return obj is Status status && Equals(status);
        }

        public bool Equals(Status other)
        {
            return string.Equals(name, other.name);
        }

        public override int GetHashCode()
        {
            return name?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Name;
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
