// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class InitialPatterns : Dictionary<DomainId, AppPattern>
    {
        public InitialPatterns()
        {
        }

        public InitialPatterns(Dictionary<DomainId, AppPattern> patterns)
            : base(patterns)
        {
        }
    }
}
