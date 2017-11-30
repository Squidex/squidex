// ==========================================================================
//  AppPatterns.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppPatterns : DictionaryWrapper<string, AppPattern>
    {
        public static readonly AppPatterns Empty = new AppPatterns();

        private AppPatterns()
            : base(ImmutableDictionary<string, AppPattern>.Empty)
        {
        }

        public AppPatterns(ImmutableDictionary<string, AppPattern> inner)
            : base(inner)
        {
        }

        [Pure]
        public AppPatterns Add(string name, AppPattern pattern)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(pattern, nameof(pattern));

            return new AppPatterns(Inner.Add(name.ToLower(), pattern));
        }

        [Pure]
        public AppPatterns Add(string name, string pattern, string defaultMessage)
        {
            var newPattern = new AppPattern
            {
                Name = name,
                Pattern = pattern,
                DefaultMessage = defaultMessage
            };

            return new AppPatterns(Inner.Add(name.ToLower(), newPattern));
        }

        [Pure]
        public AppPatterns Remove(string name)
        {
            return new AppPatterns(Inner.Remove(name.ToLower()));
        }

        [Pure]
        public AppPatterns Update(string original, string name, string pattern, string defaultMessage)
        {
            var patterns = Remove(original);
            return patterns.Add(name, pattern, defaultMessage);
        }
    }
}
