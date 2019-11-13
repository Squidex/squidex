﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Schemas.Guards
{
    public class GuardSchemaTests
    {
        private readonly Schema schema_0;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");

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
            var command = new CreateSchema { AppId = appId, Name = "INVALID NAME" };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Name is not a valid slug.", "Name"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_field_name_invalid()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
                {
                    new UpsertSchemaField
                    {
                        Name = "invalid name",
                        Properties = new StringFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                Name = "new-schema"
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field name is not a Javascript property name.",
                    "Fields[1].Name"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_field_properties_null()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = null!,
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                Name = "new-schema"
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field properties is required.",
                    "Fields[1].Properties"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_field_properties_not_valid()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new StringFieldProperties { MinLength = 10, MaxLength = 5 },
                        Partitioning = Partitioning.Invariant.Key
                    }
                },
                Name = "new-schema"
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Max length must be greater or equal to min length.",
                    "Fields[1].Properties.MinLength",
                    "Fields[1].Properties.MaxLength"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_field_partitioning_not_valid()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
                {
                    new UpsertSchemaField
                    {
                        Name = "field1",
                        Properties = new StringFieldProperties(),
                        Partitioning = "INVALID"
                    }
                },
                Name = "new-schema"
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Partitioning is not a valid value.",
                    "Fields[1].Partitioning"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_fields_contains_duplicate_name()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
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
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field 'field1' has been added twice.",
                    "Fields"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_name_invalid()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new List<UpsertSchemaNestedField>
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
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field name is not a Javascript property name.",
                    "Fields[1].Nested[1].Name"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_properties_null()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new List<UpsertSchemaNestedField>
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
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field properties is required.",
                    "Fields[1].Nested[1].Properties"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_is_array()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new List<UpsertSchemaNestedField>
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
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Nested field cannot be array fields.",
                    "Fields[1].Nested[1].Properties"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_properties_not_valid()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new List<UpsertSchemaNestedField>
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
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Max length must be greater or equal to min length.",
                    "Fields[1].Nested[1].Properties.MinLength",
                    "Fields[1].Nested[1].Properties.MaxLength"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_nested_field_have_duplicate_names()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
                {
                    new UpsertSchemaField
                    {
                        Name = "array",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = Partitioning.Invariant.Key,
                        Nested = new List<UpsertSchemaNestedField>
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
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field 'nested1' has been added twice.",
                    "Fields[1].Nested"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_ui_field_is_invalid()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
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
                FieldsInLists = new FieldNames("field1"),
                FieldsInReferences = new FieldNames("field1"),
                Name = "new-schema"
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("UI field cannot be hidden.",
                    "Fields[1].IsHidden"),
                new ValidationError("UI field cannot be disabled.",
                    "Fields[1].IsDisabled"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInLists[1]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInReferences[1]"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_invalid_lists_field_are_used()
        {
            var command = new CreateSchema
            {
                Fields = new List<UpsertSchemaField>
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
                FieldsInLists = new FieldNames(null!, null!, "field3", "field1", "field1", "field4"),
                FieldsInReferences = null,
                Name = "new-schema"
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field is required.",
                    "FieldsInLists[1]"),
                new ValidationError("Field is required.",
                    "FieldsInLists[2]"),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInLists[3]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInLists[6]"),
                new ValidationError("Field 'field1' has been added twice.",
                    "FieldsInLists"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_invalid_references_field_are_used()
        {
            var command = new CreateSchema
            {
                Fields = new List<UpsertSchemaField>
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
                FieldsInReferences = new FieldNames(null!, null!, "field3", "field1", "field1", "field4"),
                Name = "new-schema"
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field is required.",
                    "FieldsInReferences[1]"),
                new ValidationError("Field is required.",
                    "FieldsInReferences[2]"),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInReferences[3]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInReferences[6]"),
                new ValidationError("Field 'field1' has been added twice.",
                    "FieldsInReferences"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_references_contains_meta_field()
        {
            var command = new CreateSchema
            {
                FieldsInLists = null,
                FieldsInReferences = new FieldNames("meta.id"),
                Name = "new-schema"
            };

            ValidationAssert.Throws(() => GuardSchema.CanCreate(command),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInReferences[1]"));
        }

        [Fact]
        public void CanCreate_should_not_throw_exception_if_command_is_valid()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<UpsertSchemaField>
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
                        Nested = new List<UpsertSchemaNestedField>
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
                FieldsInLists = new FieldNames("field1", "meta.id"),
                FieldsInReferences = new FieldNames("field1"),
                Name = "new-schema"
            };

            GuardSchema.CanCreate(command);
        }

        [Fact]
        public void CanConfigureUIFields_should_throw_exception_if_invalid_lists_field_are_used()
        {
            var command = new ConfigureUIFields
            {
                FieldsInLists = new FieldNames(null!, null!, "field3", "field1", "field1", "field4"),
                FieldsInReferences = null
            };

            ValidationAssert.Throws(() => GuardSchema.CanConfigureUIFields(schema_0, command),
                new ValidationError("Field is required.",
                    "FieldsInLists[1]"),
                new ValidationError("Field is required.",
                    "FieldsInLists[2]"),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInLists[3]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInLists[6]"),
                new ValidationError("Field 'field1' has been added twice.",
                    "FieldsInLists"));
        }

        [Fact]
        public void CanConfigureUIFields_should_throw_exception_if_invalid_references_field_are_used()
        {
            var command = new ConfigureUIFields
            {
                FieldsInLists = null,
                FieldsInReferences = new FieldNames(null!, null!, "field3", "field1", "field1", "field4")
            };

            ValidationAssert.Throws(() => GuardSchema.CanConfigureUIFields(schema_0, command),
                new ValidationError("Field is required.",
                    "FieldsInReferences[1]"),
                new ValidationError("Field is required.",
                    "FieldsInReferences[2]"),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInReferences[3]"),
                new ValidationError("Field cannot be an UI field.",
                    "FieldsInReferences[6]"),
                new ValidationError("Field 'field1' has been added twice.",
                    "FieldsInReferences"));
        }

        [Fact]
        public void CanConfigureUIFields_should_throw_exception_if_references_contains_meta_field()
        {
            var command = new ConfigureUIFields
            {
                FieldsInLists = null,
                FieldsInReferences = new FieldNames("meta.id")
            };

            ValidationAssert.Throws(() => GuardSchema.CanConfigureUIFields(schema_0, command),
                new ValidationError("Field is not part of the schema.",
                    "FieldsInReferences[1]"));
        }

        [Fact]
        public void CanConfigureUIFields_should_not_throw_exception_if_command_is_valid()
        {
            var command = new ConfigureUIFields
            {
                FieldsInLists = new FieldNames("field1", "meta.id"),
                FieldsInReferences = new FieldNames("field2")
            };

            GuardSchema.CanConfigureUIFields(schema_0, command);
        }

        [Fact]
        public void CanPublish_should_throw_exception_if_already_published()
        {
            var command = new PublishSchema();

            var schema_1 = schema_0.Publish();

            Assert.Throws<DomainException>(() => GuardSchema.CanPublish(schema_1, command));
        }

        [Fact]
        public void CanPublish_should_not_throw_exception_if_not_published()
        {
            var command = new PublishSchema();

            GuardSchema.CanPublish(schema_0, command);
        }

        [Fact]
        public void CanUnpublish_should_throw_exception_if_already_unpublished()
        {
            var command = new UnpublishSchema();

            Assert.Throws<DomainException>(() => GuardSchema.CanUnpublish(schema_0, command));
        }

        [Fact]
        public void CanUnpublish_should_not_throw_exception_if_already_published()
        {
            var command = new UnpublishSchema();

            var schema_1 = schema_0.Publish();

            GuardSchema.CanUnpublish(schema_1, command);
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_contains_invalid_id()
        {
            var command = new ReorderFields { FieldIds = new List<long> { 1, 3 } };

            ValidationAssert.Throws(() => GuardSchema.CanReorder(schema_0, command),
                new ValidationError("Field ids do not cover all fields.", "FieldIds"));
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_do_not_covers_all_fields()
        {
            var command = new ReorderFields { FieldIds = new List<long> { 1 } };

            ValidationAssert.Throws(() => GuardSchema.CanReorder(schema_0, command),
                new ValidationError("Field ids do not cover all fields.", "FieldIds"));
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_null()
        {
            var command = new ReorderFields { FieldIds = null! };

            ValidationAssert.Throws(() => GuardSchema.CanReorder(schema_0, command),
                new ValidationError("Field ids is required.", "FieldIds"));
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_parent_field_not_found()
        {
            var command = new ReorderFields { FieldIds = new List<long> { 1, 2 }, ParentFieldId = 99 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchema.CanReorder(schema_0, command));
        }

        [Fact]
        public void CanReorder_should_not_throw_exception_if_field_ids_are_valid()
        {
            var command = new ReorderFields { FieldIds = new List<long> { 1, 2, 4 } };

            GuardSchema.CanReorder(schema_0, command);
        }

        [Fact]
        public void CanConfigurePreviewUrls_should_throw_exception_if_preview_urls_null()
        {
            var command = new ConfigurePreviewUrls { PreviewUrls = null! };

            ValidationAssert.Throws(() => GuardSchema.CanConfigurePreviewUrls(command),
                new ValidationError("Preview Urls is required.", "PreviewUrls"));
        }

        [Fact]
        public void CanConfigurePreviewUrls_should_not_throw_exception_if_valid()
        {
            var command = new ConfigurePreviewUrls { PreviewUrls = new Dictionary<string, string>() };

            GuardSchema.CanConfigurePreviewUrls(command);
        }

        [Fact]
        public void CanChangeCategory_should_not_throw_exception()
        {
            var command = new ChangeCategory();

            GuardSchema.CanChangeCategory(schema_0, command);
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteSchema();

            GuardSchema.CanDelete(schema_0, command);
        }

        private static StringFieldProperties ValidProperties()
        {
            return new StringFieldProperties { MinLength = 10, MaxLength = 20 };
        }
    }
}
