// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.TestHelpers
{
    public static class JsonHelper
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
                    new ClaimsPrincipalConverter(),
                    new DomainIdConverter(),
                    new EnvelopeHeadersConverter(),
                    new FilterConverter(),
                    new InstantConverter(),
                    new JsonValueConverter(),
                    new LanguageConverter(),
                    new NamedDomainIdConverter(),
                    new NamedGuidIdConverter(),
                    new NamedLongIdConverter(),
                    new NamedStringIdConverter(),
                    new PropertyPathConverter(),
                    new RefTokenConverter(),
                    new StringEnumConverter()),

                TypeNameHandling = TypeNameHandling.Auto
            };
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
