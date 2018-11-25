// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppPatterns : ArrayDictionary<Guid, AppPattern>
    {
        public static readonly AppPatterns Empty = new AppPatterns();

        private AppPatterns()
        {
        }

        public AppPatterns(KeyValuePair<Guid, AppPattern>[] items)
            : base(items)
        {
        }

        [Pure]
        public AppPatterns Remove(Guid id)
        {
            return new AppPatterns(Without(id));
        }

        [Pure]
        public AppPatterns Add(Guid id, string name, string pattern, string message)
        {
            var newPattern = new AppPattern(name, pattern, message);

            if (ContainsKey(id))
            {
                throw new ArgumentException("Id already exists.", nameof(id));
            }

            return new AppPatterns(With(id, newPattern));
        }

        [Pure]
        public AppPatterns Update(Guid id, string name, string pattern, string message)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNullOrEmpty(pattern, nameof(pattern));

            if (!TryGetValue(id, out var appPattern))
            {
                return this;
            }

            return new AppPatterns(With(id, appPattern.Update(name, pattern, message)));
        }
    }
}
