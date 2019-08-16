// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core
{
    public abstract class Named
    {
        public string Name { get; }

        protected Named(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            Name = name;
        }
    }
}
