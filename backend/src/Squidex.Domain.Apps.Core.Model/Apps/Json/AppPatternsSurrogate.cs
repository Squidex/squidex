// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppPatternsSurrogate : Dictionary<DomainId, AppPattern>, ISurrogate<AppPatterns>
    {
        public void FromSource(AppPatterns source)
        {
            foreach (var (key, pattern) in source)
            {
                Add(key, pattern);
            }
        }

        public AppPatterns ToSource()
        {
            if (Count == 0)
            {
                return AppPatterns.Empty;
            }

            return new AppPatterns(this);
        }
    }
}
