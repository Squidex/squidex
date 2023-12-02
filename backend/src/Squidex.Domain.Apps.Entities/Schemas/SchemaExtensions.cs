// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using StaticNamedId = Squidex.Infrastructure.NamedId;

namespace Squidex.Domain.Apps.Entities.Schemas;

public static class SchemaExtensions
{
    public static NamedId<DomainId> NamedId(this Schema schema)
    {
        return StaticNamedId.Of(schema.Id, schema.Name);
    }

    public static string EscapePartition(this string value)
    {
        return value.Replace('-', '_');
    }
}
