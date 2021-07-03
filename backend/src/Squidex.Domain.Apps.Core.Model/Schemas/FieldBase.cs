// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class FieldBase
    {
        public long Id { get; }

        public string Name { get; }

        protected FieldBase(long id, string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.GreaterThan(id, 0, nameof(id));

            Id = id;

            Name = name;
        }
    }
}
