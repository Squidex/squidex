// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Schemas;

public class SchemaCommandsTests
{
    [Fact]
    public void Should_convert_upsert_command_with_defaults()
    {
        var command = new SynchronizeSchema
        {
            Fields =
            [
                new UpsertSchemaField
                {
                    Name = "myString",
                    IsDisabled = true,
                    IsHidden = true,
                    IsLocked = true,
                    Properties = new StringFieldProperties
                    {
                        IsRequired = true
                    },
                    Partitioning = "language"
                },
            ],
            Category = "myCategory"
        };

        var field = Fields.String(1, "myString", Partitioning.Language, new StringFieldProperties
        {
            IsRequired = true
        }) with
        {
            IsDisabled = true,
            IsHidden = true,
            IsLocked = true
        };

        var expected = new Schema
        {
            Name = "my-schema",
            Properties = new SchemaProperties(),
            FieldsInLists = FieldNames.Create(),
            FieldsInReferences = FieldNames.Create(),
            Scripts = new SchemaScripts(),
            PreviewUrls = ReadonlyDictionary.Empty<string, string>(),
            Category = "myCategory"
        };

        expected = expected.AddField(field);

        var actual = command.BuildSchema("my-schema", SchemaType.Default);

        actual.Should().BeEquivalentTo(expected, opts => opts.Excluding(x => x.AppId).Excluding(x => x.UniqueId));
    }

    [Fact]
    public void Should_convert_upsert_command()
    {
        var command = new SynchronizeSchema
        {
            Properties = new SchemaProperties
            {
                Hints = "MyHints"
            },
            IsPublished = true,
            Fields =
            [
                new UpsertSchemaField
                {
                    Name = "myString",
                    IsDisabled = true,
                    IsHidden = true,
                    IsLocked = true,
                    Properties = new StringFieldProperties
                    {
                        IsRequired = true
                    },
                    Partitioning = "language"
                },
            ],
            FieldsInLists = FieldNames.Create(
                "lastModified",
                "lastModifiedBy",
                "data.myString"
            ),
            FieldsInReferences = FieldNames.Create(
                "data.myString"
            ),
            Scripts = new SchemaScripts
            {
                Change = "change-script"
            },
            PreviewUrls = new Dictionary<string, string>
            {
                ["mobile"] = "http://mobile"
            }.ToReadonlyDictionary(),
            Category = "myCategory"
        };

        var field = Fields.String(1, "myString", Partitioning.Language, new StringFieldProperties
        {
            IsRequired = true
        }) with
        {
            IsDisabled = true,
            IsHidden = true,
            IsLocked = true
        };

        var expected = new Schema
        {
            Name = "my-schema",
            Properties = new SchemaProperties
            {
                Hints = "MyHints"
            },
            IsPublished = true,
            FieldsInLists = FieldNames.Create(
                "lastModified",
                "lastModifiedBy",
                "data.myString"
            ),
            FieldsInReferences = FieldNames.Create(
                "data.myString"
            ),
            Scripts = new SchemaScripts
            {
                Change = "change-script"
            },
            PreviewUrls = new Dictionary<string, string>
            {
                ["mobile"] = "http://mobile"
            }.ToReadonlyDictionary(),
            Category = "myCategory"
        };

        expected = expected.AddField(field);

        var actual = command.BuildSchema("my-schema", SchemaType.Default);

        actual.Should().BeEquivalentTo(expected, opts => opts.Excluding(x => x.AppId).Excluding(x => x.UniqueId));
    }
}
