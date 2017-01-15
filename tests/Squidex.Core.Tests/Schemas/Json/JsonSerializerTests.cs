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
        private readonly SchemaJsonSerializer sut;

        static JsonSerializerTests()
        {
            TypeNameRegistry.Map(typeof(FieldRegistry).GetTypeInfo().Assembly);

            serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            serializerSettings.SerializationBinder = new TypeNameSerializationBinder();
        }

        public JsonSerializerTests()
        {
            sut = new SchemaJsonSerializer(new FieldRegistry(), serializerSettings);
        }

        [Fact]
        public void Should_serialize_and_deserialize_schema()
        {
            var schema =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new StringField(1, "field1", 
                        new StringFieldProperties { Label = "Field1", Pattern = "[0-9]{3}" })).DisableField(1)
                    .AddOrUpdateField(new NumberField(2, "field2", 
                        new NumberFieldProperties { Hints = "Hints" }))
                    .AddOrUpdateField(new BooleanField(3, "field3", 
                        new BooleanFieldProperties())).HideField(2)
                    .Publish();

            var deserialized = sut.Deserialize(sut.Serialize(schema));

            deserialized.ShouldBeEquivalentTo(schema);
        }
    }
}
