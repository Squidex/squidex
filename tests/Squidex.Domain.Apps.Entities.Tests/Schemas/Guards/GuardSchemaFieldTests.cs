// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Schemas.Guards
{
    public class GuardSchemaFieldTests
    {
        private readonly Schema schema_0;
        private readonly StringFieldProperties validProperties = new StringFieldProperties();
        private readonly StringFieldProperties invalidProperties = new StringFieldProperties { MinLength = 10, MaxLength = 5 };

        public GuardSchemaFieldTests()
        {
            schema_0 =
                new Schema("my-schema")
                    .AddField(new StringField(1, "field1", Partitioning.Invariant))
                    .AddField(new StringField(2, "field2", Partitioning.Invariant));
        }

        [Fact]
        public void CanHide_should_throw_exception_if_already_hidden()
        {
            var command = new HideField { FieldId = 1 };

            var schema_1 = schema_0.HideField(1);

            Assert.Throws<DomainException>(() => GuardSchemaField.CanHide(schema_1, command));
        }

        [Fact]
        public void CanHide_should_throw_exception_if_not_found()
        {
            var command = new HideField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanHide(schema_0, command));
        }

        [Fact]
        public void CanHide_hould_not_throw_exception_if_visible()
        {
            var command = new HideField { FieldId = 1 };

            GuardSchemaField.CanHide(schema_0, command);
        }

        [Fact]
        public void CanDisable_should_throw_exception_if_already_disabled()
        {
            var command = new DisableField { FieldId = 1 };

            var schema_1 = schema_0.DisableField(1);

            Assert.Throws<DomainException>(() => GuardSchemaField.CanDisable(schema_1, command));
        }

        [Fact]
        public void CanDisable_should_throw_exception_if_not_found()
        {
            var command = new DisableField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanDisable(schema_0, command));
        }

        [Fact]
        public void CanDisable_Should_not_throw_exception_if_enabled()
        {
            var command = new DisableField { FieldId = 1 };

            GuardSchemaField.CanDisable(schema_0, command);
        }

        [Fact]
        public void CanShow_should_throw_exception_if_already_shown()
        {
            var command = new ShowField { FieldId = 1 };

            Assert.Throws<DomainException>(() => GuardSchemaField.CanShow(schema_0, command));
        }

        [Fact]
        public void CanShow_should_throw_exception_if_not_found()
        {
            var command = new ShowField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanShow(schema_0, command));
        }

        [Fact]
        public void CanShow_should_not_throw_exception_if_hidden()
        {
            var command = new ShowField { FieldId = 1 };

            var schema_1 = schema_0.HideField(1);

            GuardSchemaField.CanShow(schema_1, command);
        }

        [Fact]
        public void CanEnable_should_throw_exception_if_already_enabled()
        {
            var command = new EnableField { FieldId = 1 };

            Assert.Throws<DomainException>(() => GuardSchemaField.CanEnable(schema_0, command));
        }

        [Fact]
        public void CanEnable_should_throw_exception_if_not_found()
        {
            var command = new EnableField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanEnable(schema_0, command));
        }

        [Fact]
        public void CanEnable_should_not_throw_exception_if_disabled()
        {
            var command = new EnableField { FieldId = 1 };

            var schema_1 = schema_0.DisableField(1);

            GuardSchemaField.CanEnable(schema_1, command);
        }

        [Fact]
        public void CanLock_should_throw_exception_if_already_locked()
        {
            var command = new LockField { FieldId = 1 };

            var schema_1 = schema_0.LockField(1);

            Assert.Throws<DomainException>(() => GuardSchemaField.CanLock(schema_1, command));
        }

        [Fact]
        public void LockField_should_throw_exception_if_not_found()
        {
            var command = new LockField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanLock(schema_0, command));
        }

        [Fact]
        public void CanLock_should_not_throw_exception_if_not_locked()
        {
            var command = new LockField { FieldId = 1 };

            GuardSchemaField.CanLock(schema_0, command);
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_not_found()
        {
            var command = new DeleteField { FieldId = 3 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanDelete(schema_0, command));
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_locked()
        {
            var command = new DeleteField { FieldId = 1 };

            var schema_1 = schema_0.LockField(1);

            Assert.Throws<DomainException>(() => GuardSchemaField.CanDelete(schema_1, command));
        }

        [Fact]
        public void CanDelete_should_not_throw_exception_if_not_locked()
        {
            var command = new DeleteField { FieldId = 1 };

            GuardSchemaField.CanDelete(schema_0, command);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_locked()
        {
            var command = new UpdateField { FieldId = 1, Properties = new StringFieldProperties() };

            var schema_1 = schema_0.LockField(1);

            Assert.Throws<DomainException>(() => GuardSchemaField.CanUpdate(schema_1, command));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_not_locked()
        {
            var command = new UpdateField { FieldId = 1, Properties = new StringFieldProperties() };

            GuardSchemaField.CanUpdate(schema_0, command);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_properties_null()
        {
            var command = new UpdateField { FieldId = 2, Properties = null };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanUpdate(schema_0, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_properties_not_valid()
        {
            var command = new UpdateField { FieldId = 2, Properties = new StringFieldProperties { MinLength = 10, MaxLength = 5 } };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanUpdate(schema_0, command));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_field_already_exists()
        {
            var command = new AddField { Name = "field1", Properties = new StringFieldProperties() };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanAdd(schema_0, command));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_name_not_valid()
        {
            var command = new AddField { Name = "INVALID_NAME", Properties = validProperties };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanAdd(schema_0, command));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_properties_not_valid()
        {
            var command = new AddField { Name = "field3", Properties = invalidProperties };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanAdd(schema_0, command));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_properties_null()
        {
            var command = new AddField { Name = "field3", Properties = null };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanAdd(schema_0, command));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_partitioning_not_valid()
        {
            var command = new AddField { Name = "field3", Partitioning = "INVALID_PARTITIONING", Properties = validProperties };

            Assert.Throws<ValidationException>(() => GuardSchemaField.CanAdd(schema_0, command));
        }

        [Fact]
        public void CanAdd_should_not_throw_exception_if_field_not_exists()
        {
            var command = new AddField { Name = "field3", Properties = new StringFieldProperties() };

            GuardSchemaField.CanAdd(schema_0, command);
        }
    }
}