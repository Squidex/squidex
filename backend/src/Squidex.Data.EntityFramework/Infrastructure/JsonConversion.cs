// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure;

public static class JsonConversion
{
    public static PropertyBuilder<T> AsJsonString<T>(this PropertyBuilder<T> propertyBuilder, IJsonSerializer jsonSerializer)
        where T : class
    {
        var converter = new ValueConverter<T, string>(
            v => jsonSerializer.Serialize(v, false),
            v => jsonSerializer.Deserialize<T>(v, null)!
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<DomainId> AsString(this PropertyBuilder<DomainId> propertyBuilder)
    {
        var converter = new ValueConverter<DomainId, string>(
            v => v.ToString()!,
            v => DomainId.Create(v)
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<DomainId?> AsString(this PropertyBuilder<DomainId?> propertyBuilder)
    {
        var converter = new ValueConverter<DomainId?, string?>(
            v => v != null ? v.ToString()! : null,
            v => v != null ? DomainId.Create(v) : null
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<RefToken> AsRefToken(this PropertyBuilder<RefToken> propertyBuilder)
    {
        var converter = new ValueConverter<RefToken, string>(
            v => v.ToString(),
            v => RefToken.Parse(v)
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<Instant> AsDateTimeOffset(this PropertyBuilder<Instant> propertyBuilder)
    {
        var converter = new ValueConverter<Instant, DateTimeOffset>(
            v => v.ToDateTimeOffset(),
            v => Instant.FromDateTimeOffset(v)
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }
}
