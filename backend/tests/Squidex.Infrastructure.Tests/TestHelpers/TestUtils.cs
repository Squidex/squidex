// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Claims;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.TestHelpers
{
    public static class TestUtils
    {
        public static readonly IJsonSerializer DefaultSerializer = CreateSerializer();

        public static IJsonSerializer CreateSerializer(TypeNameRegistry? typeNameRegistry = null)
        {
            var serializerSettings = DefaultSettings(typeNameRegistry);

            return new NewtonsoftJsonSerializer(serializerSettings);
        }

        public static JsonSerializerSettings DefaultSettings(TypeNameRegistry? typeNameRegistry = null)
        {
            return new JsonSerializerSettings
            {
                SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry ?? new TypeNameRegistry()),

                ContractResolver = new ConverterContractResolver(
                    new SurrogateConverter<ClaimsPrincipal, ClaimsPrinicpalSurrogate>(),
                    new EnvelopeHeadersConverter(),
                    new JsonValueConverter(),
                    new SurrogateConverter<FilterNode<IJsonValue>, JsonFilterSurrogate>(),
                    new StringEnumConverter()),

                TypeNameHandling = TypeNameHandling.Auto
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public static T SerializeAndDeserialize<T>(this T value)
        {
            var json = DefaultSerializer.Serialize(Tuple.Create(value));

            return DefaultSerializer.Deserialize<Tuple<T>>(json).Item1;
        }

        public static T Deserialize<T>(string value)
        {
            return DefaultSerializer.Deserialize<Tuple<T>>($"{{ \"Item1\": \"{value}\" }}").Item1;
        }

        public static T Deserialize<T>(object value)
        {
            return DefaultSerializer.Deserialize<Tuple<T>>($"{{ \"Item1\": {value} }}").Item1;
        }
    }
}
