// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public static class TestSchemas
{
    public static readonly Schema Default;
    public static readonly Schema Reference1;
    public static readonly Schema Reference2;
    public static readonly Schema Singleton;
    public static readonly Schema Component;

    static TestSchemas()
    {
        var enums = ReadonlyList.Create("EnumA", "EnumB", "EnumC");

        var jsonSchema = @"
                type JsonObject {
                    rootString: String
                    rootInt: Int
                    rootFloat: Float
                    rootBoolean: Boolean,
                    rootArray: [String]
                    rootObject: JsonNested
                }

                type JsonNested {
                    nestedString: String
                    nestedInt: Int
                    nestedFloat: Float
                    nestedBoolean: Boolean,
                    nestedArray: [String]
                }";

        Component =
            new Schema
            {
                AppId = TestApp.DefaultId,
                Id = DomainId.NewGuid(),
                IsPublished = true,
                IsDeleted = false,
                Name = "my-component",
                Type = SchemaType.Default,
            }
            .AddString(1, "component-field", Partitioning.Invariant);

        Reference1 =
            new Schema
            {
                AppId = TestApp.DefaultId,
                Id = DomainId.NewGuid(),
                IsPublished = true,
                IsDeleted = false,
                Name = "my-reference1",
                Type = SchemaType.Default,
            }
            .AddString(1, "reference1-field", Partitioning.Invariant);

        Reference2 =
            new Schema
            {
                AppId = TestApp.DefaultId,
                Id = DomainId.NewGuid(),
                IsPublished = true,
                IsDeleted = false,
                Name = "my-reference2",
                Type = SchemaType.Default,
            }
            .AddString(1, "reference2-field", Partitioning.Invariant);

        Singleton =
            new Schema
            {
                AppId = TestApp.DefaultId,
                Id = DomainId.NewGuid(),
                IsPublished = true,
                IsDeleted = false,
                Name = "my-singleton",
                Type = SchemaType.Singleton,
            }
            .AddString(1, "singleton-field", Partitioning.Invariant);

        Default =
            new Schema
            {
                AppId = TestApp.DefaultId,
                Id = DomainId.NewGuid(),
                IsPublished = true,
                IsDeleted = false,
                Name = "my-schema",
                Type = SchemaType.Default,
            }
            .AddJson(1, "my-json", Partitioning.Invariant,
                new JsonFieldProperties())
            .AddJson(2, "my-json2", Partitioning.Invariant,
                new JsonFieldProperties { GraphQLSchema = jsonSchema })
            .AddString(3, "my-string", Partitioning.Invariant,
                new StringFieldProperties())
            .AddString(4, "my-string-enum", Partitioning.Invariant,
                new StringFieldProperties { AllowedValues = enums, CreateEnum = true })
            .AddString(5, "my-localized-string", Partitioning.Language,
                new StringFieldProperties())
            .AddNumber(6, "my-number", Partitioning.Invariant,
                new NumberFieldProperties())
            .AddAssets(7, "my-assets", Partitioning.Invariant,
                new AssetsFieldProperties())
            .AddBoolean(8, "my-boolean", Partitioning.Invariant,
                new BooleanFieldProperties())
            .AddDateTime(9, "my-datetime", Partitioning.Invariant,
                new DateTimeFieldProperties())
            .AddReferences(10, "my-references", Partitioning.Invariant,
                new ReferencesFieldProperties { SchemaId = Reference1.Id })
            .AddReferences(11, "my-union", Partitioning.Invariant,
                new ReferencesFieldProperties())
            .AddGeolocation(12, "my-geolocation", Partitioning.Invariant,
                new GeolocationFieldProperties())
            .AddComponent(13, "my-component", Partitioning.Invariant,
                new ComponentFieldProperties { SchemaId = Component.Id })
            .AddComponents(14, "my-components", Partitioning.Invariant,
                new ComponentsFieldProperties { SchemaIds = ReadonlyList.Create(Reference1.Id, Reference2.Id, Component.Id) })
            .AddTags(15, "my-tags", Partitioning.Invariant,
                new TagsFieldProperties())
            .AddTags(16, "my-tags-enum", Partitioning.Invariant,
                new TagsFieldProperties { AllowedValues = enums, CreateEnum = true })
            .AddString(17, "my-embeds", Partitioning.Invariant,
                new StringFieldProperties { IsEmbeddable = true, SchemaIds = ReadonlyList.Create(Reference1.Id, Reference2.Id) })
            .AddRichText(18, "my-richtext", Partitioning.Invariant,
                new RichTextFieldProperties { SchemaIds = ReadonlyList.Create(Reference1.Id, Reference2.Id) })
            .AddArray(100, "my-array", Partitioning.Invariant, f => f
                .AddBoolean(121, "nested-boolean",
                    new BooleanFieldProperties())
                .AddNumber(122, "nested-number",
                    new NumberFieldProperties()))
            .SetScripts(new SchemaScripts { Query = "<query-script>" });
    }
}
