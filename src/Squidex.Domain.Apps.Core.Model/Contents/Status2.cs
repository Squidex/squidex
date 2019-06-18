// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using System;

namespace Squidex.Domain.Apps.Core.Contents
{
    public struct Status2 : IEquatable<Status2>
    {
        public static readonly Status2 Published = new Status2("Published");

        public string Name { get; }

        public Status2(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            Name = name;
        }

        public override bool Equals(object obj)
        {
            return obj is Status2 status && Equals(status);
        }

        public bool Equals(Status2 other)
        {
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
