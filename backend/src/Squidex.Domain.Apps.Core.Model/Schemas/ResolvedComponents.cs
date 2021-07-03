// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class ResolvedComponents : ImmutableDictionary<DomainId, Schema>
    {
        public static readonly ResolvedComponents Empty = new ResolvedComponents();

        private ResolvedComponents()
        {
        }

        public ResolvedComponents(IDictionary<DomainId, Schema> inner)
            : base(inner)
        {
        }

        public Schema? Get(DomainId schemaId)
        {
            return this.GetOrDefault(schemaId);
        }
    }
}
