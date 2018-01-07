// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Squidex.Infrastructure.TestHelpers
{
    public static class JsonHelper
    {
        public static void SerializeAndDeserialize<T>(this T value, IContractResolver contractResolver)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                NullValueHandling = NullValueHandling.Include
            };

            var result = JsonConvert.SerializeObject(Tuple.Create(value), serializerSettings);
            var output = JsonConvert.DeserializeObject<Tuple<T>>(result, serializerSettings);

            Assert.Equal(value, output.Item1);
        }

        public static void SerializeAndDeserialize<T>(this T value, JsonConverter converter)
        {
            var output = SerializeAndDeserializeAndReturn(value, converter);

            Assert.Equal(value, output);
        }

        public static T SerializeAndDeserializeAndReturn<T>(this T value, JsonConverter converter)
        {
            var serializerSettings = CreateSettings(converter);

            var result = JsonConvert.SerializeObject(Tuple.Create(value), serializerSettings);
            var output = JsonConvert.DeserializeObject<Tuple<T>>(result, serializerSettings);

            return output.Item1;
        }

        public static void DoesNotDeserialize<T>(string value, JsonConverter converter)
        {
            var serializerSettings = CreateSettings(converter);

            Assert.ThrowsAny<JsonException>(() => JsonConvert.DeserializeObject<Tuple<T>>($"{{ \"Item1\": \"{value}\" }}", serializerSettings));
        }

        private static JsonSerializerSettings CreateSettings(JsonConverter converter)
        {
            var serializerSettings = new JsonSerializerSettings();

            if (converter != null)
            {
                serializerSettings.Converters.Add(converter);
            }

            serializerSettings.NullValueHandling = NullValueHandling.Include;
            serializerSettings.TypeNameHandling = TypeNameHandling.Auto;

            return serializerSettings;
        }
    }
}
