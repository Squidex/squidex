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
            var inv = Partitioning.Invariant;

            var schema = new Schema("user");

            schema = schema.Publish();
            schema = schema.Update(new SchemaProperties { Hints = "The User" });

            schema = schema.AddField(new JsonField(1, "my-json", inv,
                new JsonFieldProperties()));

            schema = schema.AddField(new AssetsField(2, "my-assets", inv,
                new AssetsFieldProperties()));

            schema = schema.AddField(new StringField(3, "my-string1", inv,
                new StringFieldProperties { Label = "My String1", IsRequired = true, AllowedValues = ImmutableList.Create("a", "b") }));

            schema = schema.AddField(new StringField(4, "my-string2", inv,
                new StringFieldProperties { Hints = "My String1" }));

            schema = schema.AddField(new NumberField(5, "my-number", inv,
                new NumberFieldProperties { MinValue = 1, MaxValue = 10 }));

            schema = schema.AddField(new BooleanField(6, "my-boolean", inv,
                new BooleanFieldProperties()));

            schema = schema.AddField(new DateTimeField(7, "my-datetime", inv,
                new DateTimeFieldProperties { Editor = DateTimeFieldEditor.DateTime }));

            schema = schema.AddField(new DateTimeField(8, "my-date", inv,
                new DateTimeFieldProperties { Editor = DateTimeFieldEditor.Date }));

            schema = schema.AddField(new GeolocationField(9, "my-geolocation", inv,
                new GeolocationFieldProperties()));

            schema = schema.AddField(new ReferencesField(10, "my-references", inv,
                new ReferencesFieldProperties()));

            schema = schema.AddField(new TagsField(11, "my-tags", Partitioning.Language,
                new TagsFieldProperties()));

            schema = schema.HideField(7);
            schema = schema.LockField(8);
            schema = schema.DisableField(9);

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
