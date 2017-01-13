// ==========================================================================
//  Cloneable.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Core.Schemas
{
    public abstract class Cloneable
    {
        protected T Clone<T>(Action<T> updater) where T : Cloneable
        {
            var clone = (T)MemberwiseClone();

            updater(clone);

            return clone;
        }
    }
}
