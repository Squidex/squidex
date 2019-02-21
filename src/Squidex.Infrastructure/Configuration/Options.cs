// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration
{
    public sealed class Options : Dictionary<string, Action>
    {
        public Options()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
