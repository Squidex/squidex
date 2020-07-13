// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class AppPattern : Named
    {
        public string Pattern { get; }

        public string? Message { get; }

        public AppPattern(string name, string pattern, string? message = null)
            : base(name)
        {
            Guard.NotNullOrEmpty(pattern, nameof(pattern));

            Pattern = pattern;

            Message = message;
        }

        [Pure]
        public AppPattern Update(string? name, string? pattern, string? message)
        {
            return new AppPattern(name.Or(Name), pattern.Or(Pattern), message);
        }
    }
}
