// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure.Json;

#pragma warning disable RECS0015 // If an extension method is called as static method convert it to method syntax

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

    public static PropertyBuilder<RefToken> AsString(this PropertyBuilder<RefToken> propertyBuilder)
    {
        var converter = new ValueConverter<RefToken, string>(
            v => v.ToString(),
            v => RefToken.Parse(v)
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<NamedId<DomainId>> AsString(this PropertyBuilder<NamedId<DomainId>> propertyBuilder)
    {
        var converter = new ValueConverter<NamedId<DomainId>, string>(
            v => v.ToString(),
            v => NamedId<DomainId>.Parse(v, ParseDomainId)
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<T> AsString<T>(this PropertyBuilder<T> propertyBuilder) where T : struct
    {
        var converter = new ValueConverter<T, string>(
            v => v.ToString()!,
            v => Enum.Parse<T>(v, true)
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<HashSet<string>> AsString(this PropertyBuilder<HashSet<string>> propertyBuilder)
    {
        var converter = new ValueConverter<HashSet<string>, string>(
            v => TagsConverter.ToString(v),
            v => TagsConverter.ToSet(v)
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    public static PropertyBuilder<UniqueContentId> AsString(this PropertyBuilder<UniqueContentId> propertyBuilder)
    {
        var converter = new ValueConverter<UniqueContentId, string>(
            v => v.ToParseableString(),
            v => v.ToUniqueContentId()
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

    public static PropertyBuilder<Instant?> AsDateTimeOffset(this PropertyBuilder<Instant?> propertyBuilder)
    {
        var converter = new ValueConverter<Instant?, DateTimeOffset?>(
            v => v != null ? v.Value.ToDateTimeOffset() : null,
            v => v != null ? Instant.FromDateTimeOffset(v.Value) : null
        );

        propertyBuilder.HasConversion(converter);
        return propertyBuilder;
    }

    private static bool ParseDomainId(ReadOnlySpan<char> value, out DomainId result)
    {
        result = DomainId.Create(new string(value));

        return true;
    }
}
