// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class InitialPatterns : Dictionary<Guid, AppPattern>
    {
        public InitialPatterns()
        {
        }

        public InitialPatterns(Dictionary<Guid, AppPattern> patterns)
            : base(patterns)
        {
        }
    }
}
