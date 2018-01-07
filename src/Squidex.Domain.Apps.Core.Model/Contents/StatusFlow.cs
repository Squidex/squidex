// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;

namespace Squidex.Domain.Apps.Core.Contents
{
    public static class StatusFlow
    {
        private static readonly Dictionary<Status, Status[]> Flow = new Dictionary<Status, Status[]>
        {
            [Status.Draft] = new[] { Status.Published, Status.Archived },
            [Status.Archived] = new[] { Status.Draft },
            [Status.Published] = new[] { Status.Draft, Status.Archived }
        };

        public static bool Exists(Status status)
        {
            return Flow.ContainsKey(status);
        }

        public static bool CanChange(Status status, Status toStatus)
        {
            return Flow.TryGetValue(status, out var state) && state.Contains(toStatus);
        }
    }
}
