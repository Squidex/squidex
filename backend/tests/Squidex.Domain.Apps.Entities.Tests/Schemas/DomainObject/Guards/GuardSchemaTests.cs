// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject.Guards
{
    public class GuardSchemaTests : IClassFixture<TranslationsFixture>
    {
        private readonly Schema schema_0;
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");

        public GuardSchemaTests()
        {
            schema_0 =
                new Schema("my-schema")
                    .AddString(1, "field1", Partitioning.Invariant)
                    .AddString(2, "field2", Partitioning.Invariant)
                    .AddUI(4, "field4", Partitioning.Invariant);
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_name_not_valid()
        {
            var command = CreateCommand(new CreateSchema { Name = "INVALID NAME" });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Name is not a valid slug.", "Name"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_field_name_invalid()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "invalid name",
                        Properties = new StringFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Name is not a Javascript property name.",
                    "Fields[0].Name"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_field_properties_null()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = null!,
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Properties is required.",
                    "Fields[0].Properties"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_field_properties_not_valid()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new StringFieldProperties { MinLength = 10, MaxLength = 5 },
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Max length must be greater or equal to min length.",
                    "Fields[0].Properties.MinLength",
                    "Fields[0].Properties.MaxLength"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_field_partitioning_not_valid()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new StringFieldProperties(),
                        Partitioning = "INVALID"
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Partitioning is not a valid value.",
                    "Fields[0].Partitioning"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_fields_contains_duplicate_name()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new StringFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key
                    },
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new StringFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field 'field1' has been added twice.",
                    "Fields"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_name_invalid()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new[]
                        {
                            new UpsertSchemaNestedField
                            {
                                Name = "invalid name",
                                Properties = new StringFieldProperties()
                            }
                        }
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Name is not a Javascript property name.",
                    "Fields[0].Nested[0].Name"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_properties_null()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new[]
                        {
                            new UpsertSchemaNestedField
                            {
                                Name = "nested1",
                                Properties = null!
                            }
                        }
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Properties is required.",
                    "Fields[0].Nested[0].Properties"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_is_array()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new[]
                        {
                            new UpsertSchemaNestedField
                            {
                                Name = "nested1",
                                Properties = new ArrayFieldProperties()
                            }
                        }
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Nested field cannot be array fields.",
                    "Fields[0].Nested[0].Properties"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_properties_not_valid()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new[]
                        {
                            new UpsertSchemaNestedField
                            {
                                Name = "nested1",
                                Properties = new StringFieldProperties { MinLength = 10, MaxLength = 5 }
                            }
                        }
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Max length must be greater or equal to min length.",
                    "Fields[0].Nested[0].Properties.MinLength",
                    "Fields[0].Nested[0].Properties.MaxLength"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_have_duplicate_names()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new[]
                        {
                            new UpsertSchemaNestedField
                            {
                                Name = "nested1",
                                Properties = new StringFieldProperties()
                            },
                            new UpsertSchemaNestedField
                            {
                                Name = "nested1",
                                Properties = new StringFieldProperties()
                            }
                        }
                    }
                },
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field 'nested1' has been added twice.",
                    "Fields[0].Nested"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_ui_field_is_invalid()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new UIFieldProperties(),
                        IsHidden = true,
                        IsDisabled = true,
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                FieldsInLists = FieldNames.Create("field1"),
                FieldsInReferences = FieldNames.Create("field1"),
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("UI field cannot be hidden.",
                    "Fields[0].IsHidden"),
                new ValidationError("UI field cannot be disabled.",
                    "Fields[0].IsDisabled"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInLists[0]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInReferences[0]"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_invalid_lists_field_are_used()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new StringFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key
                    },
                    new UpsertSchemaField
                    {
                        Name = "field4",
                        Properties = new UIFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                FieldsInLists = FieldNames.Create(null!, null!, "field3", "field1", "field1", "field4"),
                FieldsInReferences = null,
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field is required.",
                    "FieldsInLists[0]"),
                new ValidationError("Field is required.",
                    "FieldsInLists[1]"),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInLists[2]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInLists[5]"),
                new ValidationError("Field 'field1' has been added twice.",
                    "FieldsInLists"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_invalid_references_field_are_used()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new StringFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key
                    },
                    new UpsertSchemaField
                    {
                        Name = "field4",
                        Properties = new UIFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                FieldsInLists = null,
                FieldsInReferences = FieldNames.Create(null!, null!, "field3", "field1", "field1", "field4"),
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field is required.",
                    "FieldsInReferences[0]"),
                new ValidationError("Field is required.",
                    "FieldsInReferences[1]"),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInReferences[2]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInReferences[5]"),
                new ValidationError("Field 'field1' has been added twice.",
                    "FieldsInReferences"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_references_contains_meta_field()
        {
            var command = CreateCommand(new CreateSchema
            {
                FieldsInLists = null,
                FieldsInReferences = FieldNames.Create("meta.id"),
                Name = "new-schema"
            });

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInReferences[0]"));
        }

        [Fact]
        public void CanCreate_should_not_throw_exception_if_command_is_valid()
        {
            var command = CreateCommand(new CreateSchema
            {
                Fields = new[]
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new StringFieldProperties(),
                        IsHidden = true,
                        IsDisabled = true,
                        Partitioning = Partitioning.Invariant.Key
                    },
                    new UpsertSchemaField
                    {
                        Name = "field2",
                        Properties = ValidProperties(),
                        Partitioning = Partitioning.Invariant.Key
                    },
                    new UpsertSchemaField
                    {
                        Name = "field3",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new[]
                        {
                            new UpsertSchemaNestedField
                            {
                                Name = "nested1",
                                Properties = ValidProperties()
                            },
                            new UpsertSchemaNestedField
                            {
                                Name = "nested2",
                                Properties = ValidProperties()
                            }
                        }
                    }
                },
                FieldsInLists = FieldNames.Create("field1", "meta.id"),
                FieldsInReferences = FieldNames.Create("field1"),
                Name = "new-schema"
            });

            GuardSchema.CanCreate(command);
        }

        [Fact]
        public void CanConfigureUIFields_should_throw_exception_if_invalid_lists_field_are_used()
        {
            var command = new ConfigureUIFields
            {
                FieldsInLists = FieldNames.Create(null!, null!, "field3", "field1", "field1", "field4"),
                FieldsInReferences = null
            };

            ValidationAssert.Throws(() => GuardSchema.CanConfigureUIFields(command, schema_0),
                new ValidationError("Field is required.",
                    "FieldsInLists[0]"),
                new ValidationError("Field is required.",
                    "FieldsInLists[1]"),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInLists[2]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInLists[5]"),
                new ValidationError("Field 'field1' has been added twice.",
                    "FieldsInLists"));
        }

        [Fact]
        public void CanConfigureUIFields_should_throw_exception_if_invalid_references_field_are_used()
        {
            var command = new ConfigureUIFields
            {
                FieldsInLists = null,
                FieldsInReferences = FieldNames.Create(null!, null!, "field3", "field1", "field1", "field4")
            };

            ValidationAssert.Throws(() => GuardSchema.CanConfigureUIFields(command, schema_0),
                new ValidationError("Field is required.",
                    "FieldsInReferences[0]"),
                new ValidationError("Field is required.",
                    "FieldsInReferences[1]"),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInReferences[2]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInReferences[5]"),
                new ValidationError("Field 'field1' has been added twice.",
                    "FieldsInReferences"));
        }

        [Fact]
        public void CanConfigureUIFields_should_throw_exception_if_references_contains_meta_field()
        {
            var command = new ConfigureUIFields
            {
                FieldsInLists = null,
                FieldsInReferences = FieldNames.Create("meta.id")
            };

            ValidationAssert.Throws(() => GuardSchema.CanConfigureUIFields(command, schema_0),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInReferences[0]"));
        }

        [Fact]
        public void CanConfigureUIFields_should_not_throw_exception_if_command_is_valid()
        {
            var command = new ConfigureUIFields
            {
                FieldsInLists = FieldNames.Create("field1", "meta.id"),
                FieldsInReferences = FieldNames.Create("field2")
            };

            GuardSchema.CanConfigureUIFields(command, schema_0);
        }

        [Fact]
        public void CanConfigureFieldRules_should_throw_exception_if_field_rules_are_invalid()
        {
            var command = new ConfigureFieldRules
            {
                FieldRules = new[]
                {
                    new FieldRuleCommand { Field = "field", Action = (FieldRuleAction)5 },
                    new FieldRuleCommand()
                }
            };

            ValidationAssert.Throws(() => GuardSchema.CanConfigureFieldRules(command),
                new ValidationError("Action is not a valid value.",
                    "FieldRules[0].Action"),
                new ValidationError("Field is required.",
                    "FieldRules[1].Field"));
        }

        [Fact]
        public void CanConfigureFieldRules_should_not_throw_exception_if_field_rules_are_valid()
        {
            var command = new ConfigureFieldRules
            {
                FieldRules = new[]
                {
                    new FieldRuleCommand { Field = "field1", Action = FieldRuleAction.Disable, Condition = "a == b" },
                    new FieldRuleCommand { Field = "field2" }
                }
            };

            GuardSchema.CanConfigureFieldRules(command);
        }

        [Fact]
        public void CanConfigureFieldRules_should_not_throw_exception_if_field_rules_are_null()
        {
            var command = new ConfigureFieldRules
            {
                FieldRules = null
            };

            GuardSchema.CanConfigureFieldRules(command);
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_contains_invalid_id()
        {
            var command = new ReorderFields { FieldIds = new[] { 1L, 3L } };

            ValidationAssert.Throws(() => GuardSchema.CanReorder(command, schema_0),
                new ValidationError("Field ids do not cover all fields.", "FieldIds"));
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_do_not_covers_all_fields()
        {
            var command = new ReorderFields { FieldIds = new[] { 1L } };

            ValidationAssert.Throws(() => GuardSchema.CanReorder(command, schema_0),
                new ValidationError("Field ids do not cover all fields.", "FieldIds"));
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_null()
        {
            var command = new ReorderFields { FieldIds = null! };

            ValidationAssert.Throws(() => GuardSchema.CanReorder(command, schema_0),
                new ValidationError("Field IDs is required.", "FieldIds"));
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_parent_field_not_found()
        {
            var command = new ReorderFields { FieldIds = new[] { 1L, 2L }, ParentFieldId = 99 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchema.CanReorder(command, schema_0));
        }

        [Fact]
        public void CanReorder_should_not_throw_exception_if_field_ids_are_valid()
        {
            var command = new ReorderFields { FieldIds = new[] { 1L, 2L, 4L } };

            GuardSchema.CanReorder(command, schema_0);
        }

        [Fact]
        public void CanConfigurePreviewUrls_should_throw_exception_if_preview_urls_null()
        {
            var command = new ConfigurePreviewUrls { PreviewUrls = null! };

            ValidationAssert.Throws(() => GuardSchema.CanConfigurePreviewUrls(command),
                new ValidationError("Preview URLs is required.", "PreviewUrls"));
        }

        [Fact]
        public void CanConfigurePreviewUrls_should_not_throw_exception_if_valid()
        {
            var command = new ConfigurePreviewUrls { PreviewUrls = ImmutableDictionary.Empty<string, string>() };

            GuardSchema.CanConfigurePreviewUrls(command);
        }

        private CreateSchema CreateCommand(CreateSchema command)
        {
            command.AppId = appId;

            return command;
        }

        private static StringFieldProperties ValidProperties()
        {
            return new StringFieldProperties { MinLength = 10, MaxLength = 20 };
        }
    }
}
