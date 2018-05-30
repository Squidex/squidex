// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Schemas.Guards
{
    public class GuardSchemaTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly Schema schema_0;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");

        public GuardSchemaTests()
        {
            schema_0 =
                new Schema("my-schema")
                    .AddString(1, "field1", Partitioning.Invariant)
                    .AddString(2, "field2", Partitioning.Invariant);

            A.CallTo(() => appProvider.GetSchemaAsync(A<Guid>.Ignored, "new-schema"))
                .Returns(Task.FromResult<ISchemaEntity>(null));
        }

        [Fact]
        public Task CanCreate_should_throw_exception_if_name_not_valid()
        {
            var command = new CreateSchema { AppId = appId, Name = "INVALID NAME" };

            return Assert.ThrowsAsync<ValidationException>(() => GuardSchema.CanCreate(command, appProvider));
        }

        [Fact]
        public Task CanCreate_should_throw_exception_if_name_already_in_use()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(A<Guid>.Ignored, "new-schema"))
                .Returns(Task.FromResult(A.Fake<ISchemaEntity>()));

            var command = new CreateSchema { AppId = appId, Name = "new-schema" };

            return Assert.ThrowsAsync<ValidationException>(() => GuardSchema.CanCreate(command, appProvider));
        }

        [Fact]
        public Task CanCreate_should_throw_exception_if_fields_not_valid()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = null,
                        Properties = null,
                        Partitioning = "invalid"
                    },
                    new CreateSchemaField
                    {
                        Name = null,
                        Properties = InvalidProperties(),
                        Partitioning = "invalid"
                    },
                    new CreateSchemaField
                    {
                        Name = null,
                        Properties = new ArrayFieldProperties(),
                        Partitioning = "invalid",
                        Nested = new List<CreateSchemaNestedField>
                        {
                            new CreateSchemaNestedField
                            {
                                Name = null,
                                Properties = InvalidProperties()
                            },
                            new CreateSchemaNestedField
                            {
                                Name = null,
                                Properties = InvalidProperties()
                            }
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = null,
                        Properties = InvalidProperties(),
                        Partitioning = "invalid",
                        Nested = new List<CreateSchemaNestedField>
                        {
                            new CreateSchemaNestedField
                            {
                                Name = null,
                                Properties = InvalidProperties()
                            },
                            new CreateSchemaNestedField
                            {
                                Name = null,
                                Properties = InvalidProperties()
                            }
                        }
                    }
                },
                Name = "new-schema"
            };

            return Assert.ThrowsAsync<ValidationException>(() => GuardSchema.CanCreate(command, appProvider));
        }

        [Fact]
        public Task CanCreate_should_throw_exception_if_fields_contain_duplicate_names()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "field1",
                        Properties = ValidProperties(),
                        Partitioning = "invariant"
                    },
                    new CreateSchemaField
                    {
                        Name = "field1",
                        Properties = ValidProperties(),
                        Partitioning = "invariant"
                    },
                    new CreateSchemaField
                    {
                        Name = "field1",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = "invariant",
                        Nested = new List<CreateSchemaNestedField>
                        {
                            new CreateSchemaNestedField
                            {
                                Name = "nested1",
                                Properties = ValidProperties()
                            },
                            new CreateSchemaNestedField
                            {
                                Name = "nested1",
                                Properties = ValidProperties()
                            }
                        }
                    }
                },
                Name = "new-schema"
            };

            return Assert.ThrowsAsync<ValidationException>(() => GuardSchema.CanCreate(command, appProvider));
        }

        [Fact]
        public Task CanCreate_should_not_throw_exception_if_command_is_valid()
        {
            var command = new CreateSchema
            {
                AppId = appId,
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "field1",
                        Properties = ValidProperties(),
                        Partitioning = "invariant"
                    },
                    new CreateSchemaField
                    {
                        Name = "field2",
                        Properties = ValidProperties(),
                        Partitioning = "invariant"
                    },
                    new CreateSchemaField
                    {
                        Name = "field3",
                        Properties = new ArrayFieldProperties(),
                        Partitioning = "invariant",
                        Nested = new List<CreateSchemaNestedField>
                        {
                            new CreateSchemaNestedField
                            {
                                Name = "nested1",
                                Properties = ValidProperties()
                            },
                            new CreateSchemaNestedField
                            {
                                Name = "nested2",
                                Properties = ValidProperties()
                            }
                        }
                    }
                },
                Name = "new-schema"
            };

            return GuardSchema.CanCreate(command, appProvider);
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

            Assert.Throws<ValidationException>(() => GuardSchema.CanReorder(schema_0, command));
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_do_not_covers_all_fields()
        {
            var command = new ReorderFields { FieldIds = new List<long> { 1 } };

            Assert.Throws<ValidationException>(() => GuardSchema.CanReorder(schema_0, command));
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_null()
        {
            var command = new ReorderFields { FieldIds = null };

            Assert.Throws<ValidationException>(() => GuardSchema.CanReorder(schema_0, command));
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
            var command = new ReorderFields { FieldIds = new List<long> { 1, 2 } };

            GuardSchema.CanReorder(schema_0, command);
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

        private static StringFieldProperties InvalidProperties()
        {
            return new StringFieldProperties { MinLength = 20, MaxLength = 10 };
        }
    }
}
