// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
