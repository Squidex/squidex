// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace Squidex.Infrastructure.Queries
{
    public sealed class PropertyPath : ReadOnlyCollection<string>
    {
        private static readonly char[] Separators = { '.', '/' };

        public PropertyPath(IList<string> items)
            : base(items)
        {
            if (items.Count == 0)
            {
                throw new ArgumentException("Path cannot be empty.", nameof(items));
            }
        }

        public static implicit operator PropertyPath(string path)
        {
            return new PropertyPath(path?.Split(Separators, StringSplitOptions.RemoveEmptyEntries).ToList()!);
        }

        public static implicit operator PropertyPath(string[] path)
        {
            return new PropertyPath(path?.ToList()!);
        }

        public static implicit operator PropertyPath(List<string> path)
        {
            return new PropertyPath(path);
        }

        public static implicit operator PropertyPath(ImmutableList<string> path)
        {
            return new PropertyPath(path);
        }

        public override string ToString()
        {
            return string.Join(".", this);
        }
    }
}
