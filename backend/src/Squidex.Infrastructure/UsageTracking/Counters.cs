// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class Counters : Dictionary<string, double>
    {
        public double Get(string name)
        {
            if (name == null)
            {
                return 0;
            }

            TryGetValue(name, out var value);

            return value;
        }
    }
}
