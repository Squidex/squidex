// ==========================================================================
//  JsonSerializerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Immutable;
using FluentAssertions;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public class JsonSerializerTests
    {
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();
        private readonly SchemaJsonSerializer sut;

        public JsonSerializerTests()
        {
            serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            serializerSettings.SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry);

            sut = new SchemaJsonSerializer(new FieldRegistry(typeNameRegistry), serializerSettings);
        }

        [Fact]
        public void Should_serialize_and_deserialize_schema()
        {
            var schema =
                Schema.Create("user", new SchemaProperties { Hints = "The User" })
                    .AddOrUpdateField(new JsonField(1, "my-json", Partitioning.Invariant,
                        new JsonFieldProperties()))
                    .AddOrUpdateField(new AssetsField(2, "my-assets", Partitioning.Invariant,
                        new AssetsFieldProperties()))
                    .AddOrUpdateField(new StringField(3, "my-string1", Partitioning.Language,
                        new StringFieldProperties { Label = "My String1", IsRequired = true, AllowedValues = ImmutableList.Create("a", "b") }))
                    .AddOrUpdateField(new StringField(4, "my-string2", Partitioning.Invariant,
                        new StringFieldProperties { Hints = "My String1" }))
                    .AddOrUpdateField(new NumberField(5, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties { MinValue = 1, MaxValue = 10 }))
                    .AddOrUpdateField(new BooleanField(6, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties()))
                    .AddOrUpdateField(new DateTimeField(7, "my-datetime", Partitioning.Invariant,
                        new DateTimeFieldProperties { Editor = DateTimeFieldEditor.DateTime }))
                    .AddOrUpdateField(new DateTimeField(8, "my-date", Partitioning.Invariant,
                        new DateTimeFieldProperties { Editor = DateTimeFieldEditor.Date }))
                    .AddOrUpdateField(new ReferencesField(9, "my-references", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = Guid.NewGuid() }))
                    .AddOrUpdateField(new GeolocationField(10, "my-geolocation", Partitioning.Invariant,
                        new GeolocationFieldProperties()))
                    .HideField(1)
                    .LockField(2)
                    .DisableField(3)
                    .Publish();

            var deserialized = sut.Deserialize(sut.Serialize(schema));

            deserialized.ShouldBeEquivalentTo(schema);
        }
    }
}
