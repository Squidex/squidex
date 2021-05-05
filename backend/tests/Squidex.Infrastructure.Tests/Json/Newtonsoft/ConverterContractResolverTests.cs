// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using NodaTime.Text;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

#pragma warning disable xUnit1004 // Test methods should not be skipped

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public class ConverterContractResolverTests
    {
        public class MyClass
        {
            [JsonConverter(typeof(TodayConverter))]
            public Instant MyProperty { get; set; }
        }

        public sealed class TodayConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                writer.WriteValue("TODAY");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Instant);
            }
        }

        [Fact]
        public void Should_respect_property_converter()
        {
            var value = Instant.FromUtc(2012, 12, 10, 9, 8, 45);

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new ConverterContractResolver(new NodaPatternConverter<Instant>(InstantPattern.ExtendedIso)),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.None
            };

            var serializer = new NewtonsoftJsonSerializer(serializerSettings);

            var json = serializer.Serialize(new MyClass { MyProperty = value }, false);

            Assert.Equal(@"{""myProperty"":""TODAY""}", json);
        }

        [Fact(Skip = "No idea why it does not work in some cases.")]
        public void Should_ignore_other_converters()
        {
            var value = Instant.FromUtc(2012, 12, 10, 9, 8, 45);

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new ConverterContractResolver(new NodaPatternConverter<Instant>(InstantPattern.ExtendedIso)),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.None
            };

            serializerSettings.Converters.Add(new TodayConverter());

            var serializer = new NewtonsoftJsonSerializer(serializerSettings);
            var serialized = serializer.Deserialize<Instant>(serializer.Serialize(value, true))!;

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_instant()
        {
            var value = Instant.FromUtc(2012, 12, 10, 9, 8, 45);

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_dictionary()
        {
            var value = new Dictionary<string, string> { ["Description"] = "value" };

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }
    }
}
