// ==========================================================================
//  Freezable.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public abstract class Freezable
    {
        public bool IsFrozen { get; private set; }

        protected void ThrowIfFrozen()
        {
            if (IsFrozen)
            {
                throw new InvalidOperationException("Object is frozen");
            }
        }

        public void Freeze()
        {
            IsFrozen = true;
        }
    }
}
