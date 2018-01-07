// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class CloneableBase
    {
        protected T Clone<T>(Action<T> updater) where T : CloneableBase
        {
            var clone = (T)MemberwiseClone();

            updater(clone);

            return clone;
        }
    }
}
