// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppPatterns : ImmutableDictionary<DomainId, AppPattern>
    {
        public static readonly AppPatterns Empty = new AppPatterns();

        private AppPatterns()
        {
        }

        public AppPatterns(Dictionary<DomainId, AppPattern> inner)
            : base(inner)
        {
        }

        [Pure]
        public AppPatterns Remove(DomainId id)
        {
            return Without<AppPatterns>(id);
        }

        [Pure]
        public AppPatterns Add(DomainId id, string name, string pattern, string? message = null)
        {
            var newPattern = new AppPattern(name, pattern, message);

            return With<AppPatterns>(id, newPattern);
        }

        [Pure]
        public AppPatterns Update(DomainId id, string? name = null, string? pattern = null, string? message = null)
        {
            if (!TryGetValue(id, out var appPattern))
            {
                return this;
            }

            var newPattern = appPattern.Update(name, pattern, message);

            return With<AppPatterns>(id, newPattern);
        }
    }
}
