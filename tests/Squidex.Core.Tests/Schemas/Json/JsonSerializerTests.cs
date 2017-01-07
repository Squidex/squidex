// ==========================================================================
//  JsonSerializerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Core.Schemas.Json
{
    public class JsonSerializerTests
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();

        static JsonSerializerTests()
        {
            TypeNameRegistry.Map(typeof(FieldRegistry).GetTypeInfo().Assembly);

            serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            serializerSettings.SerializationBinder = new TypeNameSerializationBinder();
        }

        [Fact]
        public void Should_serialize_and_deserialize_schema()
        {
            var schema =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new StringField(1, "field1", new StringFieldProperties { Label = "Field1", Pattern = "[0-9]{3}" }))
                    .AddOrUpdateField(new NumberField(2, "field2", new NumberFieldProperties { Hints = "Hints" }))
                    .AddOrUpdateField(new BooleanField(2, "field2", new BooleanFieldProperties()))
                    .Publish()
                    .HideField(2)
                    .DisableField(1);


            var sut = new SchemaJsonSerializer(new FieldRegistry(), serializerSettings);

            var token = sut.Serialize(schema);

            var deserialized = sut.Deserialize(token);

            deserialized.ShouldBeEquivalentTo(schema);
        }
    }
}
