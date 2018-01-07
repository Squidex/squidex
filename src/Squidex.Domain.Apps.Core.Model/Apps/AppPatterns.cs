// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================
using System;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppPatterns : DictionaryWrapper<Guid, AppPattern>
    {
        public static readonly AppPatterns Empty = new AppPatterns();

        private AppPatterns()
            : base(ImmutableDictionary<Guid, AppPattern>.Empty)
        {
        }

        public AppPatterns(ImmutableDictionary<Guid, AppPattern> inner)
            : base(inner)
        {
        }

        [Pure]
        public AppPatterns Add(Guid id, string name, string pattern, string message)
        {
            var newPattern = new AppPattern(name, pattern, message);

            return new AppPatterns(Inner.Add(id, newPattern));
        }

        [Pure]
        public AppPatterns Remove(Guid id)
        {
            return new AppPatterns(Inner.Remove(id));
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

            return new AppPatterns(Inner.SetItem(id, appPattern.Update(name, pattern, message)));
        }
    }
}
