// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed class ResolvedComponents : ReadonlyDictionary<DomainId, Schema>
{
    public static readonly ResolvedComponents Empty = new ResolvedComponents();

    private ResolvedComponents()
    {
    }

    public ResolvedComponents(IDictionary<DomainId, Schema> inner)
        : base(inner)
    {
    }

    public ResolvedComponents Resolve(IEnumerable<DomainId>? schemaIds)
    {
        var result = (Dictionary<DomainId, Schema>?)null;

        if (schemaIds != null)
        {
            foreach (var schemaId in schemaIds)
            {
                if (TryGetValue(schemaId, out var schema))
                {
                    result ??= new Dictionary<DomainId, Schema>();
                    result[schemaId] = schema;
                }
            }
        }

        if (result == null)
        {
            return Empty;
        }

        return new ResolvedComponents(result);
    }
}
