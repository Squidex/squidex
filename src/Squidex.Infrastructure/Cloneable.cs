// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public abstract class Cloneable
    {
        protected T Clone<T>(Action<T> updater) where T : Cloneable
        {
            var clone = (T)MemberwiseClone();

            updater(clone);

            clone.OnCloned();

            return clone;
        }

        protected virtual void OnCloned()
        {
        }
    }
}
