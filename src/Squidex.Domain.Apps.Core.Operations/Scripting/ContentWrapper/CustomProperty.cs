// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint.Runtime.Descriptors;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public abstract class CustomProperty : PropertyDescriptor
    {
        protected CustomProperty()
            : base(PropertyFlag.CustomJsValue)
        {
            Enumerable = true;

            Writable = true;

            Configurable = true;
        }
    }
}
