// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
