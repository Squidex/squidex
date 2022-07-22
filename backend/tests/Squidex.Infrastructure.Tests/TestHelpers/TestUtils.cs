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
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Json.System;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Reflection;

#pragma warning disable SYSLIB0011 // Type or member is obsolete

namespace Squidex.Infrastructure.TestHelpers
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

        public static IJsonSerializer CreateSerializer(params JsonConverter[] converters)
        {
            var serializerSettings = DefaultOptions(converters);

            return new SystemJsonSerializer(serializerSettings);
        }

        public static JsonSerializerOptions DefaultOptions(params JsonConverter[] converters)
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            options.Converters.Add(new StringConverter<PropertyPath>(x => x));
            options.Converters.Add(new JsonValueConverter());
            options.Converters.Add(new ReadonlyDictionaryConverterFactory());
            options.Converters.Add(new ReadonlyListConverterFactory());
            options.Converters.Add(new SurrogateJsonConverter<ClaimsPrincipal, ClaimsPrincipalSurrogate>());
            options.Converters.Add(new SurrogateJsonConverter<FilterNode<JsonValue>, JsonFilterSurrogate>());
            options.Converters.Add(new StringConverter<CompareOperator>());
            options.Converters.Add(new StringConverter<DomainId>());
            options.Converters.Add(new StringConverter<NamedId<DomainId>>());
            options.Converters.Add(new StringConverter<NamedId<Guid>>());
            options.Converters.Add(new StringConverter<NamedId<string>>());
            options.Converters.Add(new StringConverter<Language>());
            options.Converters.Add(new StringConverter<RefToken>());
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.AddRange(converters);

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
            var stream = new MemoryStream();

            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, new ObjectHolder<T> { Value1 = value, Value2 = value });

                writer.Flush();
            }

            stream.Position = 0;

            using (var reader = new BsonBinaryReader(stream))
            {
                var result = BsonSerializer.Deserialize<ObjectHolder<T>>(reader);

                return result.Value1;
            }
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
    }
}
