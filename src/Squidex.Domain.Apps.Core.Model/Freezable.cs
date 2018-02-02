// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core
{
    public abstract class Freezable : IFreezable
    {
        private bool isFrozen;

        public bool IsFrozen
        {
            get { return isFrozen; }
        }

        protected void CheckIfFrozen()
        {
            if (isFrozen)
            {
                throw new InvalidOperationException("Object is frozen");
            }
        }

        public virtual void Freeze()
        {
            isFrozen = true;
        }
    }
}
