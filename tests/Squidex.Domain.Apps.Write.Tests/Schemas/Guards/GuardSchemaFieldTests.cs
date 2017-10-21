// ==========================================================================
//  GuardSchemaFieldTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Domain.Apps.Write.Schemas.Guards;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Schemas
{
    public class GuardSchemaFieldTests
    {
        private readonly Schema schema = new Schema("my-schema");

        public GuardSchemaFieldTests()
        {
            schema.AddField(new StringField(1, "field1", Partitioning.Invariant));
            schema.AddField(new StringField(2, "field2", Partitioning.Invariant));
        }

        [Fact]
        public void CanHide_should_throw_exception_if_already_hidden()
        {
            var command = new HideField { FieldId = 1 };

            schema.FieldsById[1].Hide();

            Assert.Throws<DomainException>(() => GuardSchemaField.CanHide(schema, command));
        }

        [Fact]
        public void CanHide_should_throw_exception_if_not_found()
        {
            var command = new HideField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanHide(schema, command));
        }

        [Fact]
        public void CanHide_hould_not_throw_exception_if_visible()
        {
            var command = new HideField { FieldId = 1 };

            GuardSchemaField.CanHide(schema, command);
        }

        [Fact]
        public void CanDisable_should_throw_exception_if_already_disabled()
        {
            var command = new DisableField { FieldId = 1 };

            schema.FieldsById[1].Disable();

            Assert.Throws<DomainException>(() => GuardSchemaField.CanDisable(schema, command));
        }

        [Fact]
        public void CanDisable_should_throw_exception_if_not_found()
        {
            var command = new DisableField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanDisable(schema, command));
        }

        [Fact]
        public void CanDisable_Should_not_throw_exception_if_enabled()
        {
            var command = new DisableField { FieldId = 1 };

            GuardSchemaField.CanDisable(schema, command);
        }

        [Fact]
        public void CanShow_should_throw_exception_if_already_shown()
        {
            var command = new ShowField { FieldId = 1 };

            Assert.Throws<DomainException>(() => GuardSchemaField.CanShow(schema, command));
        }

        [Fact]
        public void CanShow_should_throw_exception_if_not_found()
        {
            var command = new ShowField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanShow(schema, command));
        }

        [Fact]
        public void CanShow_should_not_throw_exception_if_hidden()
        {
            var command = new ShowField { FieldId = 1 };

            schema.FieldsById[1].Hide();

            GuardSchemaField.CanShow(schema, command);
        }

        [Fact]
        public void CanEnable_should_throw_exception_if_already_enabled()
        {
            var command = new EnableField { FieldId = 1 };

            Assert.Throws<DomainException>(() => GuardSchemaField.CanEnable(schema, command));
        }

        [Fact]
        public void CanEnable_should_throw_exception_if_not_found()
        {
            var command = new EnableField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanEnable(schema, command));
        }

        [Fact]
        public void CanEnable_should_not_throw_exception_if_disabled()
        {
            var command = new EnableField { FieldId = 1 };

            schema.FieldsById[1].Disable();

            GuardSchemaField.CanEnable(schema, command);
        }

        [Fact]
        public void CanLock_should_throw_exception_if_already_locked()
        {
            var command = new LockField { FieldId = 1 };

            schema.FieldsById[1].Lock();

            Assert.Throws<DomainException>(() => GuardSchemaField.CanLock(schema, command));
        }

        [Fact]
        public void LockField_should_throw_exception_if_not_found()
        {
            var command = new LockField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanLock(schema, command));
        }

        [Fact]
        public void CanLock_should_not_throw_exception_if_not_locked()
        {
            var command = new LockField { FieldId = 1 };

            GuardSchemaField.CanLock(schema, command);
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_not_found()
        {
            var command = new DeleteField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanDelete(schema, command));
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_locked()
        {
            var command = new DeleteField { FieldId = 1 };

            schema.FieldsById[1].Lock();

            Assert.Throws<DomainException>(() => GuardSchemaField.CanDelete(schema, command));
        }

        [Fact]
        public void CanDelete_should_not_throw_exception_if_not_locked()
        {
            var command = new DeleteField { FieldId = 1 };

            GuardSchemaField.CanDelete(schema, command);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_locked()
        {
            var command = new UpdateField { FieldId = 1, Properties = new StringFieldProperties() };

            schema.FieldsById[1].Lock();

            Assert.Throws<DomainException>(() => GuardSchemaField.CanUpdate(schema, command));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_not_locked()
        {
            var command = new UpdateField { FieldId = 1, Properties = new StringFieldProperties() };

            GuardSchemaField.CanUpdate(schema, command);
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_field_already_exists()
        {
            var command = new AddField { Name = "field1", Properties = new StringFieldProperties() };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanAdd(schema, command));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_name_not_valid()
        {
            var command = new AddField { Name = "INVALID_NAME", Properties = new StringFieldProperties() };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanAdd(schema, command));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_properties_not_valid()
        {
            var command = new AddField { Name = "field3", Properties = new StringFieldProperties { MinLength = 10, MaxLength = 5 } };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanAdd(schema, command));
        }

        [Fact]
        public void CanAdd_should_not_throw_exception_if_field_not_exists()
        {
            var command = new AddField { Name = "field3", Properties = new StringFieldProperties() };

            GuardSchemaField.CanAdd(schema, command);
        }
    }
}