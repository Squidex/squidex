// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Infrastructure.TestHelpers
{
    public static class JsonHelper
    {
        public static IJsonSerializer DefaultSerializer(TypeNameRegistry typeNameRegistry = null)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry ?? new TypeNameRegistry()),

                ContractResolver = new ConverterContractResolver(
                    new ClaimsPrincipalConverter(),
                    new InstantConverter(),
                    new JsonValueConverter(),
                    new LanguageConverter(),
                    new NamedGuidIdConverter(),
                    new NamedLongIdConverter(),
                    new NamedStringIdConverter(),
                    new PropertiesBagConverter<EnvelopeHeaders>(),
                    new PropertiesBagConverter<PropertiesBag>(),
                    new RefTokenConverter(),
                    new StringEnumConverter()),

                TypeNameHandling = TypeNameHandling.Auto
            };

            return new NewtonsoftJsonSerializer(serializerSettings);
        }

        public static T SerializeAndDeserialize<T>(this T value)
        {
            var serializer = DefaultSerializer();

            return serializer.Deserialize<Tuple<T>>(serializer.Serialize(Tuple.Create(value))).Item1;
        }

        public static T Deserialize<T>(string value)
        {
            var serializer = DefaultSerializer();

            return serializer.Deserialize<Tuple<T>>($"{{ \"Item1\": \"{value}\" }}").Item1;
        }

        public static T Deserialize<T>(object value)
        {
            var serializer = DefaultSerializer();

            return serializer.Deserialize<Tuple<T>>($"{{ \"Item1\": {value} }}").Item1;
        }
    }
}
