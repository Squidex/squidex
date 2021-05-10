// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core
{
    public abstract record Named
    {
        public string Name { get; }

        protected Named(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            Name = name;
        }
    }
}
