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

namespace Squidex.Domain.Apps.Entities.TestHelpers
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
                    new InstantConverter(),
                    new EnvelopeHeadersConverter(),
                    new FilterConverter(),
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
            return DefaultSerializer.Deserialize<Tuple<T>>(DefaultSerializer.Serialize(Tuple.Create(value))).Item1;
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
