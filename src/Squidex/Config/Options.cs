// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Config
{
    public sealed class Options : Dictionary<string, Action>
    {
        public Options()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
