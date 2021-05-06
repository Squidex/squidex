// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Squidex.Infrastructure.Collections
{
    public class ImmutableList<T> : ReadOnlyCollection<T>, IEquatable<ImmutableList<T>>
    {
        private static readonly List<T> EmptyInner = new List<T>();

        public ImmutableList()
            : base(EmptyInner)
        {
        }

        public ImmutableList(IList<T> list)
            : base(list as IList<T> ?? list.ToList())
        {
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ImmutableList<T>);
        }

        public bool Equals(ImmutableList<T>? other)
        {
            return this.EqualsList(other);
        }

        public override int GetHashCode()
        {
            return this.SequentialHashCode();
        }
    }
}
