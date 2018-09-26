// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Domain.Apps.Core.Apps.Json;
using Squidex.Domain.Apps.Core.Rules.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Domain.Apps.Core
{
    public static class TestData
    {
        public static JsonSerializer DefaultSerializer()
        {
            var typeNameRegistry = new TypeNameRegistry();

            var serializerSettings = new JsonSerializerSettings
            {
                SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry),

                ContractResolver = new ConverterContractResolver(
                    new AppClientsConverter(),
                    new AppContributorsConverter(),
                    new AppPatternsConverter(),
                    new InstantConverter(),
                    new LanguageConverter(),
                    new LanguagesConfigConverter(),
                    new NamedGuidIdConverter(),
                    new NamedLongIdConverter(),
                    new NamedStringIdConverter(),
                    new RefTokenConverter(),
                    new RuleConverter(),
                    new SchemaConverter(new FieldRegistry(typeNameRegistry)),
                    new StringEnumConverter()),

                TypeNameHandling = TypeNameHandling.Auto
            };

            return JsonSerializer.Create(serializerSettings);
        }

        public static Schema MixedSchema()
        {
            var schema = new Schema("user")
                .Publish()
                .AddArray(101, "root-array", Partitioning.Language, f => f
                    .AddAssets(201, "nested-assets")
                    .AddBoolean(202, "nested-boolean")
                    .AddDateTime(203, "nested-datetime")
                    .AddGeolocation(204, "nested-geolocation")
                    .AddJson(205, "nested-json")
                    .AddJson(211, "nested-json2")
                    .AddNumber(206, "nested-number")
                    .AddReferences(207, "nested-references")
                    .AddString(208, "nested-string")
                    .AddTags(209, "nested-tags"))
                .AddAssets(102, "root-assets", Partitioning.Invariant,
                    new AssetsFieldProperties())
                .AddBoolean(103, "root-boolean", Partitioning.Invariant,
                    new BooleanFieldProperties())
                .AddDateTime(104, "root-datetime", Partitioning.Invariant,
                    new DateTimeFieldProperties { Editor = DateTimeFieldEditor.DateTime })
                .AddDateTime(105, "root-date", Partitioning.Invariant,
                    new DateTimeFieldProperties { Editor = DateTimeFieldEditor.Date })
                .AddGeolocation(106, "root-geolocation", Partitioning.Invariant,
                    new GeolocationFieldProperties())
                .AddJson(107, "root-json", Partitioning.Invariant,
                    new JsonFieldProperties())
                .AddNumber(108, "root-number", Partitioning.Invariant,
                    new NumberFieldProperties { MinValue = 1, MaxValue = 10 })
                .AddReferences(109, "root-references", Partitioning.Invariant,
                    new ReferencesFieldProperties())
                .AddString(110, "root-string1", Partitioning.Invariant,
                    new StringFieldProperties { Label = "My String1", IsRequired = true, AllowedValues = ImmutableList.Create("a", "b") })
                .AddString(111, "root-string2", Partitioning.Invariant,
                    new StringFieldProperties { Hints = "My String1" })
                .AddTags(112, "root-tags", Partitioning.Language,
                    new TagsFieldProperties())
                .Update(new SchemaProperties { Hints = "The User" })
                .HideField(104)
                .HideField(211, 101)
                .DisableField(109)
                .DisableField(212, 101)
                .LockField(105);

            return schema;
        }

        public static void TestFreeze(IFreezable freezable)
        {
            var sut = new AssetsFieldProperties();

            foreach (var property in sut.GetType().GetRuntimeProperties().Where(x => x.Name != "IsFrozen"))
            {
                var value =
                    property.PropertyType.GetTypeInfo().IsValueType ?
                        Activator.CreateInstance(property.PropertyType) :
                        null;

                property.SetValue(sut, value);

                var result = property.GetValue(sut);

                Assert.Equal(value, result);
            }

            sut.Freeze();

            foreach (var property in sut.GetType().GetRuntimeProperties().Where(x => x.Name != "IsFrozen"))
            {
                var value =
                    property.PropertyType.GetTypeInfo().IsValueType ?
                        Activator.CreateInstance(property.PropertyType) :
                        null;

                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        property.SetValue(sut, value);
                    }
                    catch (Exception ex)
                    {
                        throw ex.InnerException;
                    }
                });
            }
        }
    }
}
