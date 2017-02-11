// ==========================================================================
//  SquidexCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Write
{
    public abstract class SquidexCommand
    {
        public RefToken Actor { get; set; }
    }
}
