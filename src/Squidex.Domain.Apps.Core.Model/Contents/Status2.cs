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
            return base.Equals(obj);
        }

        public bool Equals(Status2 other)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
