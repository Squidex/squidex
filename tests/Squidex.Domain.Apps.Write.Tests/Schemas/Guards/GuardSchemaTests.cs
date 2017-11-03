// ==========================================================================
//  GuardSchemaTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Schemas.Guards
{
    public class GuardSchemaTests
    {
        private readonly ISchemaProvider schemas = A.Fake<ISchemaProvider>();
        private readonly Schema schema = new Schema("my-schema");
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");

        public GuardSchemaTests()
        {
            schema.AddField(new StringField(1, "field1", Partitioning.Invariant));
            schema.AddField(new StringField(2, "field2", Partitioning.Invariant));

            A.CallTo(() => schemas.FindSchemaByNameAsync(A<Guid>.Ignored, "new-schema"))
                .Returns(Task.FromResult<ISchemaEntity>(null));
        }

        [Fact]
        public Task CanCreate_should_throw_exception_if_name_not_valid()
        {
            var command = new CreateSchema { AppId = appId, Name = "INVALID NAME" };

            return Assert.ThrowsAsync<ValidationException>(() => GuardSchema.CanCreate(command, schemas));
        }

        [Fact]
        public Task CanCreate_should_throw_exception_if_name_already_in_use()
        {
            A.CallTo(() => schemas.FindSchemaByNameAsync(A<Guid>.Ignored, "new-schema"))
                .Returns(Task.FromResult(A.Fake<ISchemaEntity>()));

            var command = new CreateSchema { AppId = appId, Name = "new-schema" };

            return Assert.ThrowsAsync<ValidationException>(() => GuardSchema.CanCreate(command, schemas));
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
                    }
                },
                Name = "new-schema"
            };

            return Assert.ThrowsAsync<ValidationException>(() => GuardSchema.CanCreate(command, schemas));
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
                    }
                },
                Name = "new-schema"
            };

            return Assert.ThrowsAsync<ValidationException>(() => GuardSchema.CanCreate(command, schemas));
        }

        [Fact]
        public Task CanCreate_should_not_throw_exception_if_command_is_valid()
        {
            var command = new CreateSchema { AppId = appId, Name = "new-schema" };

            return GuardSchema.CanCreate(command, schemas);
        }

        [Fact]
        public void CanPublish_should_throw_exception_if_already_published()
        {
            var command = new PublishSchema();

            schema.Publish();

            Assert.Throws<DomainException>(() => GuardSchema.CanPublish(schema, command));
        }

        [Fact]
        public void CanPublish_should_not_throw_exception_if_not_published()
        {
            var command = new PublishSchema();

            GuardSchema.CanPublish(schema, command);
        }

        [Fact]
        public void CanUnpublish_should_throw_exception_if_already_unpublished()
        {
            var command = new UnpublishSchema();

            Assert.Throws<DomainException>(() => GuardSchema.CanUnpublish(schema, command));
        }

        [Fact]
        public void CanUnpublish_should_not_throw_exception_if_already_published()
        {
            var command = new UnpublishSchema();

            schema.Publish();

            GuardSchema.CanUnpublish(schema, command);
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_contains_invalid_id()
        {
            var command = new ReorderFields { FieldIds = new List<long> { 1, 3 } };

            Assert.Throws<ValidationException>(() => GuardSchema.CanReorder(schema, command));
        }

        [Fact]
        public void CanReorder_should_throw_exception_if_field_ids_do_not_covers_all_fields()
        {
            var command = new ReorderFields { FieldIds = new List<long> { 1 } };

            Assert.Throws<ValidationException>(() => GuardSchema.CanReorder(schema, command));
        }

        [Fact]
        public void CanReorder_should_not_throw_exception_if_field_ids_are_valid()
        {
            var command = new ReorderFields { FieldIds = new List<long> { 1, 2 } };

            GuardSchema.CanReorder(schema, command);
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteSchema();

            GuardSchema.CanDelete(schema, command);
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
