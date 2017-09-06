// ==========================================================================
//  CloneableBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class CloneableBase
    {
        protected T Clone<T>(Action<T> updater)
            where T : CloneableBase
        {
            var clone = (T)MemberwiseClone();

            updater(clone);

            return clone;
        }
    }
}
