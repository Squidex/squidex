﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppPatterns : ImmutableDictionary<Guid, AppPattern>
    {
        public static readonly AppPatterns Empty = new AppPatterns();

        private AppPatterns()
        {
        }

        public AppPatterns(Dictionary<Guid, AppPattern> inner)
            : base(inner)
        {
        }

        [Pure]
        public AppPatterns Remove(Guid id)
        {
            return Without<AppPatterns>(id);
        }

        [Pure]
        public AppPatterns Add(Guid id, string name, string pattern, string? message = null)
        {
            var newPattern = new AppPattern(name, pattern, message);

            return With<AppPatterns>(id, newPattern);
        }

        [Pure]
        public AppPatterns Update(Guid id, string? name = null, string? pattern = null, string? message = null)
        {
            if (!TryGetValue(id, out var appPattern))
            {
                return this;
            }

            return With<AppPatterns>(id, appPattern.Update(name, pattern, message));
        }
    }
}
