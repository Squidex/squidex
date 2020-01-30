// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core
{
    [Equals(DoNotAddEquals = true, DoNotAddGetHashCode = true, DoNotAddEqualityOperators = true)]
    public abstract class Freezable : IFreezable
    {
        private bool isFrozen;

        [IgnoreEquals]
        [IgnoreDuringEquals]
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
