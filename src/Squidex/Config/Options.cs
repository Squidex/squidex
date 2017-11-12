// ==========================================================================
//  Options.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
