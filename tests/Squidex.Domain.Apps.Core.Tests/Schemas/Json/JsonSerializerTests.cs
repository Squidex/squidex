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
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public class JsonSerializerTests
    {
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();

        public JsonSerializerTests()
        {
            serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            serializerSettings.SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry);
            serializerSettings.Converters.Add(new SchemaConverter(new FieldRegistry(typeNameRegistry)));
        }

        [Fact]
        public void Should_serialize_and_deserialize_schema()
        {
            var schema =
                Schema.Create("user", new SchemaProperties { Hints = "The User" })
                    .AddField(new JsonField(1, "my-json", Partitioning.Invariant,
                        new JsonFieldProperties())).HideField(1)
                    .AddField(new AssetsField(2, "my-assets", Partitioning.Invariant,
                        new AssetsFieldProperties())).LockField(2)
                    .AddField(new StringField(3, "my-string1", Partitioning.Language,
                        new StringFieldProperties { Label = "My String1", IsRequired = true, AllowedValues = ImmutableList.Create("a", "b") }))
                    .AddField(new StringField(4, "my-string2", Partitioning.Invariant,
                        new StringFieldProperties { Hints = "My String1" }))
                    .AddField(new NumberField(5, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties { MinValue = 1, MaxValue = 10 }))
                    .AddField(new BooleanField(6, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties())).DisableField(3)
                    .AddField(new DateTimeField(7, "my-datetime", Partitioning.Invariant,
                        new DateTimeFieldProperties { Editor = DateTimeFieldEditor.DateTime }))
                    .AddField(new DateTimeField(8, "my-date", Partitioning.Invariant,
                        new DateTimeFieldProperties { Editor = DateTimeFieldEditor.Date }))
                    .AddField(new ReferencesField(9, "my-references", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = Guid.NewGuid() }))
                    .AddField(new GeolocationField(10, "my-geolocation", Partitioning.Invariant,
                        new GeolocationFieldProperties()))
                    .Publish();

            var deserialized = JToken.FromObject(schema).ToObject<Schema>();

            deserialized.ShouldBeEquivalentTo(schema);
        }
    }
}
