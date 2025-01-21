// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure;

public static class JsonConversion
{
    public static PropertyBuilder<T> HasJsonValueConversion<T>(this PropertyBuilder<T> propertyBuilder, IJsonSerializer jsonSerializer)
        where T : class
    {
        var converter = new ValueConverter<T, string>(
            v => jsonSerializer.Serialize(v, false),
            v => jsonSerializer.Deserialize<T>(v, null)!
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<DomainId> HasDomainIdConverter(this PropertyBuilder<DomainId> propertyBuilder)
    {
        var converter = new ValueConverter<DomainId, string>(
            v => v.ToString()!,
            v => DomainId.Create(v)
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<DomainId?> HasDomainIdConverter(this PropertyBuilder<DomainId?> propertyBuilder)
    {
        var converter = new ValueConverter<DomainId?, string?>(
            v => v != null ? v.ToString()! : null,
            v => v != null ? DomainId.Create(v) : null
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }
}
