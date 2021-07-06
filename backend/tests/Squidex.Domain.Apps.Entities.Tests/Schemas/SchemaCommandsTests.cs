// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Collections;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaCommandsTests
    {
        [Fact]
        public void Should_convert_upsert_command()
        {
            var command = new SynchronizeSchema
            {
                IsPublished = true,
                Properties = new SchemaProperties { Hints = "MyHints" },
                Fields = new[]
                {
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
                    }
                },
                FieldsInLists = FieldNames.Create("meta.id", "myString"),
                FieldsInReferences = FieldNames.Create("myString"),
                Scripts = new SchemaScripts
                {
                    Change = "change-script"
                },
                PreviewUrls = new Dictionary<string, string>
                {
                    ["mobile"] = "http://mobile"
                }.ToImmutableDictionary(),
                Category = "myCategory"
            };

            var expected =
                new Schema("my-schema")
                    .Update(new SchemaProperties { Hints = "MyHints" })
                    .AddString(1, "myString", Partitioning.Language, new StringFieldProperties
                    {
                        IsRequired = true
                    })
                    .HideField(1).DisableField(1).LockField(1)
                    .ChangeCategory("myCategory")
                    .SetFieldsInLists(FieldNames.Create("meta.id", "myString"))
                    .SetFieldsInReferences(FieldNames.Create("myString"))
                    .SetScripts(new SchemaScripts
                    {
                        Change = "change-script"
                    })
                    .SetPreviewUrls(new Dictionary<string, string>
                    {
                        ["mobile"] = "http://mobile"
                    }.ToImmutableDictionary())
                    .Publish();

            var actual = command.BuildSchema("my-schema", SchemaType.Default);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
