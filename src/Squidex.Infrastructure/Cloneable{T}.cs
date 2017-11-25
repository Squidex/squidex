// ==========================================================================
//  Cloneable{T}.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public abstract class Cloneable<T> : Cloneable where T : Cloneable
    {
        protected T Clone(Action<T> updater)
        {
            return base.Clone(updater);
        }
    }
}
