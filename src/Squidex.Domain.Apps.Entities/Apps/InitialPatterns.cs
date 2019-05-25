// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class InitialPatterns : List<AppPattern>
    {
        public InitialPatterns()
        {
        }

        public InitialPatterns(IEnumerable<AppPattern> patterns)
            : base(patterns)
        {
        }
    }
}
