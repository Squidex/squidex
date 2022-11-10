// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using NetTopologySuite.IO.Converters;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Apps.Json;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Contents.Json;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Json.System;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Reflection;

#pragma warning disable SYSLIB0011 // Type or member is obsolete

namespace Squidex.Domain.Apps.Core.TestHelpers;

public static class TestUtils
{
    public static readonly TypeRegistry TypeRegistry = CreateTypeRegistry(typeof(TestUtils).Assembly);

    public static readonly IJsonSerializer DefaultSerializer = CreateSerializer();

    public sealed class ObjectHolder<T>
    {
        [BsonRequired]
        public T Value1 { get; set; }

        [BsonRequired]
        public T Value2 { get; set; }
    }

    static TestUtils()
    {
        SetupBson();
    }

    public static void SetupBson()
    {
        BsonDomainIdSerializer.Register();
        BsonEscapedDictionarySerializer<ContentFieldData, ContentData>.Register();
        BsonEscapedDictionarySerializer<JsonValue, ContentFieldData>.Register();
        BsonEscapedDictionarySerializer<JsonValue, JsonObject>.Register();
        BsonEscapedDictionarySerializer<JsonValue, JsonObject>.Register();
        BsonInstantSerializer.Register();
        BsonJsonConvention.Register(DefaultOptions());
        BsonJsonValueSerializer.Register();
        BsonStringSerializer<RefToken>.Register();
        BsonStringSerializer<Status>.Register();
    }

    public static TypeRegistry CreateTypeRegistry(Assembly assembly)
    {
        var typeRegistry =
            new TypeRegistry()
                .Map(new FieldTypeProvider())
                .Map(new AssemblyTypeProvider<IEvent>(assembly))
                .Map(new AssemblyTypeProvider<IEvent>(SquidexEvents.Assembly))
                .Map(new AssemblyTypeProvider<RuleAction>(assembly))
                .Map(new AssemblyTypeProvider<RuleTrigger>(assembly))
                .Map(new RuleTypeProvider());

        return typeRegistry;
    }

    public static IJsonSerializer CreateSerializer(Action<JsonSerializerOptions>? configure = null)
    {
        var serializerSettings = DefaultOptions(configure);

        return new SystemJsonSerializer(serializerSettings);
    }

    public static JsonSerializerOptions DefaultOptions(Action<JsonSerializerOptions>? configure = null)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // It is also a readonly list, so we have to register it first, so that other converters do not pick this up.
        options.Converters.Add(new StringConverter<PropertyPath>(x => x));

        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        options.Converters.Add(new GeoJsonConverterFactory());
        options.Converters.Add(new PolymorphicConverter<IEvent>(TypeRegistry));
        options.Converters.Add(new PolymorphicConverter<FieldProperties>(TypeRegistry));
        options.Converters.Add(new PolymorphicConverter<RuleAction>(TypeRegistry));
        options.Converters.Add(new PolymorphicConverter<RuleTrigger>(TypeRegistry));
        options.Converters.Add(new JsonValueConverter());
        options.Converters.Add(new ReadonlyDictionaryConverterFactory());
        options.Converters.Add(new ReadonlyListConverterFactory());
        options.Converters.Add(new SurrogateJsonConverter<ClaimsPrincipal, ClaimsPrincipalSurrogate>());
        options.Converters.Add(new SurrogateJsonConverter<FilterNode<JsonValue>, JsonFilterSurrogate>());
        options.Converters.Add(new SurrogateJsonConverter<LanguageConfig, LanguageConfigSurrogate>());
        options.Converters.Add(new SurrogateJsonConverter<LanguagesConfig, LanguagesConfigSurrogate>());
        options.Converters.Add(new SurrogateJsonConverter<Roles, RolesSurrogate>());
        options.Converters.Add(new SurrogateJsonConverter<Rule, RuleSorrgate>());
        options.Converters.Add(new SurrogateJsonConverter<Schema, SchemaSurrogate>());
        options.Converters.Add(new SurrogateJsonConverter<WorkflowStep, WorkflowStepSurrogate>());
        options.Converters.Add(new SurrogateJsonConverter<WorkflowTransition, WorkflowTransitionSurrogate>());
        options.Converters.Add(new StringConverter<CompareOperator>());
        options.Converters.Add(new StringConverter<DomainId>());
        options.Converters.Add(new StringConverter<NamedId<DomainId>>());
        options.Converters.Add(new StringConverter<NamedId<Guid>>());
        options.Converters.Add(new StringConverter<NamedId<long>>());
        options.Converters.Add(new StringConverter<NamedId<string>>());
        options.Converters.Add(new StringConverter<Language>());
        options.Converters.Add(new StringConverter<RefToken>());
        options.Converters.Add(new StringConverter<Status>());
        options.Converters.Add(new JsonStringEnumConverter());
        options.TypeInfoResolver = new PolymorphicTypeResolver(TypeRegistry);
        configure?.Invoke(options);

        return options;
    }

    public static T SerializeAndDeserializeBinary<T>(this T source)
    {
        using (var stream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, source!);

            stream.Position = 0;

            return (T)formatter.Deserialize(stream);
        }
    }

    public static T SerializeAndDeserializeBson<T>(this T value)
    {
        return SerializeAndDeserializeBson<T, T>(value);
    }

    public static TOut SerializeAndDeserializeBson<TOut, TIn>(this TIn value)
    {
        using var stream = new MemoryStream();

        using (var writer = new BsonBinaryWriter(stream))
        {
            BsonSerializer.Serialize(writer, new ObjectHolder<TIn> { Value1 = value, Value2 = value });
        }

        stream.Position = 0;

        using (var reader = new BsonBinaryReader(stream))
        {
            return BsonSerializer.Deserialize<ObjectHolder<TOut>>(reader).Value1;
        }
    }

    public static T SerializeAndDeserialize<T>(this T value)
    {
        return SerializeAndDeserialize<T, T>(value);
    }

    public static TOut SerializeAndDeserialize<TOut, TIn>(this TIn value)
    {
        var json = DefaultSerializer.Serialize(new ObjectHolder<TIn> { Value1 = value, Value2 = value });

        return DefaultSerializer.Deserialize<ObjectHolder<TOut>>(json).Value1;
    }

    public static T Deserialize<T>(string value)
    {
        var json = DefaultSerializer.Serialize(new ObjectHolder<string> { Value1 = value, Value2 = value });

        return DefaultSerializer.Deserialize<ObjectHolder<T>>(json).Value1;
    }

    public static string CleanJson(this string json)
    {
        using var document = JsonDocument.Parse(json);

        return DefaultSerializer.Serialize(document, true);
    }

    public static TEvent CreateEvent<TEvent>(Action<TEvent>? init = null) where TEvent : IEvent, new()
    {
        var actual = new TEvent();

        if (actual is SquidexEvent squidexEvent)
        {
            squidexEvent.Actor = RefToken.Client("my-client");
        }

        if (actual is AppEvent appEvent)
        {
            appEvent.AppId = NamedId.Of(DomainId.NewGuid(), "my-app");
        }

        if (actual is SchemaEvent schemaEvent)
        {
            schemaEvent.SchemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        }

        init?.Invoke(actual);

        return actual;
    }
}
