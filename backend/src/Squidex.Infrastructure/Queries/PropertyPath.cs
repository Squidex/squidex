// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure.Collections;

namespace Squidex.Infrastructure.Queries
{
    public sealed class PropertyPath : ImmutableList<string>
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
            return Create(path?.Split(Separators, StringSplitOptions.RemoveEmptyEntries).ToList());
        }

        public static implicit operator PropertyPath(string[] path)
        {
            return Create(path);
        }

        public static implicit operator PropertyPath(List<string> path)
        {
            return Create(path);
        }

        public override string ToString()
        {
            return string.Join(".", this);
        }

        private static PropertyPath Create(IEnumerable<string>? source)
        {
            var inner = source?.ToList();

            if (inner == null || inner.Count == 0)
            {
                throw new ArgumentException("Path cannot be empty.", nameof(source));
            }
            else
            {
                return new PropertyPath(inner);
            }
        }
    }
}
