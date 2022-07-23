// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
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

namespace Squidex.Domain.Apps.Core.TestHelpers
{
    public static class TestUtils
    {
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
            BsonInstantSerializer.Register();
            BsonJsonConvention.Register(DefaultOptions());
            BsonJsonValueSerializer.Register();
        }

        public static IJsonSerializer CreateSerializer(Action<JsonSerializerOptions>? configure = null)
        {
            var serializerSettings = DefaultOptions(configure);

            return new SystemJsonSerializer(serializerSettings);
        }

        public static JsonSerializerOptions DefaultOptions(Action<JsonSerializerOptions>? configure = null)
        {
            var typeNameRegistry =
                new TypeNameRegistry()
                    .Map(new FieldTypeProvider())
                    .Map(new RuleTypeProvider())
                    .MapUnmapped(typeof(TestUtils).Assembly);

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            options.Converters.Add(new InheritanceConverter<IEvent>(typeNameRegistry));
            options.Converters.Add(new InheritanceConverter<FieldProperties>(typeNameRegistry));
            options.Converters.Add(new InheritanceConverter<RuleAction>(typeNameRegistry));
            options.Converters.Add(new InheritanceConverter<RuleTrigger>(typeNameRegistry));
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
            options.Converters.Add(new StringConverter<NamedId<string>>());
            options.Converters.Add(new StringConverter<Language>());
            options.Converters.Add(new StringConverter<PropertyPath>(x => x));
            options.Converters.Add(new StringConverter<RefToken>());
            options.Converters.Add(new StringConverter<Status>());
            options.Converters.Add(new JsonStringEnumConverter());
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
            using var stream = new MemoryStream();

            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, new ObjectHolder<T> { Value1 = value, Value2 = value });
            }

            stream.Position = 0;

            using (var reader = new BsonBinaryReader(stream))
            {
                return BsonSerializer.Deserialize<ObjectHolder<T>>(reader).Value1;
            }
        }

        public static T SerializeAndDeserialize<T>(this object value)
        {
            var json = DefaultSerializer.Serialize(value);

            return DefaultSerializer.Deserialize<T>(json);
        }

        public static T SerializeAndDeserialize<T>(this T value)
        {
            var json = DefaultSerializer.Serialize(new ObjectHolder<T> { Value1 = value, Value2 = value });

            return DefaultSerializer.Deserialize<ObjectHolder<T>>(json).Value1;
        }

        public static T Deserialize<T>(string value)
        {
            var json = DefaultSerializer.Serialize(new ObjectHolder<string> { Value1 = value, Value2 = value });

            return DefaultSerializer.Deserialize<ObjectHolder<T>>(json).Value1;
        }

        public static T Deserialize<T>(object value)
        {
            var json = DefaultSerializer.Serialize(new ObjectHolder<object> { Value1 = value, Value2 = value });

            return DefaultSerializer.Deserialize<ObjectHolder<T>>(json).Value1;
        }

        public static string CleanJson(this string json)
        {
            using var document = JsonDocument.Parse(json);

            return DefaultSerializer.Serialize(document, true);
        }
    }
}
