// ==========================================================================
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
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore
#pragma warning disable SA1401 // Fields must be private

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
                    .AddString(1, "field1", Partitioning.Invariant)
                    .AddString(2, "field2", Partitioning.Invariant)
                    .AddArray(3, "field3", Partitioning.Invariant, f => f
                        .AddNumber(301, "field301"));
        }

        private static Action<Schema, T> A<T>(Action<Schema, T> method) where T : FieldCommand
        {
            return method;
        }

        private static Func<Schema, Schema> S(Func<Schema, Schema> method)
        {
            return method;
        }

        public static IEnumerable<object[]> FieldCommandData = new[]
        {
            new object[] { A<EnableField>(GuardSchemaField.CanEnable) },
            new object[] { A<DeleteField>(GuardSchemaField.CanDelete) },
            new object[] { A<DisableField>(GuardSchemaField.CanDisable) },
            new object[] { A<HideField>(GuardSchemaField.CanHide) },
            new object[] { A<LockField>(GuardSchemaField.CanLock) },
            new object[] { A<ShowField>(GuardSchemaField.CanShow) },
            new object[] { A<UpdateField>(GuardSchemaField.CanUpdate) }
        };

        public static IEnumerable<object[]> InvalidStates = new[]
        {
            new object[] { A<DisableField>(GuardSchemaField.CanDisable), S(s => s.DisableField(1)) },
            new object[] { A<EnableField>(GuardSchemaField.CanEnable),   S(s => s) },
            new object[] { A<HideField>(GuardSchemaField.CanHide),       S(s => s.HideField(1)) },
            new object[] { A<ShowField>(GuardSchemaField.CanShow),       S(s => s.LockField(1)) },
            new object[] { A<LockField>(GuardSchemaField.CanLock),       S(s => s.LockField(1)) }
        };

        public static IEnumerable<object[]> InvalidNestedStates = new[]
        {
            new object[] { A<DisableField>(GuardSchemaField.CanDisable), S(s => s.DisableField(301, 3)) },
            new object[] { A<EnableField>(GuardSchemaField.CanEnable),   S(s => s) },
            new object[] { A<HideField>(GuardSchemaField.CanHide),       S(s => s.HideField(301, 3)) },
            new object[] { A<ShowField>(GuardSchemaField.CanShow),       S(s => s) },
            new object[] { A<LockField>(GuardSchemaField.CanLock),       S(s => s.LockField(301, 3)) }
        };

        public static IEnumerable<object[]> ValidStates = new[]
        {
            new object[] { A<DisableField>(GuardSchemaField.CanDisable), S(s => s) },
            new object[] { A<EnableField>(GuardSchemaField.CanEnable),   S(s => s.DisableField(1)) },
            new object[] { A<HideField>(GuardSchemaField.CanHide),       S(s => s) },
            new object[] { A<ShowField>(GuardSchemaField.CanShow),       S(s => s.HideField(1)) }
        };

        public static IEnumerable<object[]> ValidNestedStates = new[]
        {
            new object[] { A<EnableField>(GuardSchemaField.CanEnable),   S(s => s.DisableField(301, 3)) },
            new object[] { A<DisableField>(GuardSchemaField.CanDisable), S(s => s) },
            new object[] { A<HideField>(GuardSchemaField.CanHide),       S(s => s) },
            new object[] { A<ShowField>(GuardSchemaField.CanShow),       S(s => s.HideField(301, 3)) }
        };

        [Theory]
        [MemberData(nameof(FieldCommandData))]
        public void Commands_should_throw_exception_if_field_not_found<T>(Action<Schema, T> action) where T : FieldCommand, new()
        {
            var command = new T { FieldId = 4 };

            Assert.Throws<DomainObjectNotFoundException>(() => action(schema_0, command));
        }

        [Theory]
        [MemberData(nameof(FieldCommandData))]
        public void Commands_should_throw_exception_if_parent_field_not_found<T>(Action<Schema, T> action) where T : FieldCommand, new()
        {
            var command = new T { ParentFieldId = 4, FieldId = 401 };

            Assert.Throws<DomainObjectNotFoundException>(() => action(schema_0, command));
        }

        [Theory]
        [MemberData(nameof(FieldCommandData))]
        public void Commands_should_throw_exception_if_child_field_not_found<T>(Action<Schema, T> action) where T : FieldCommand, new()
        {
            var command = new T { ParentFieldId = 3, FieldId = 302 };

            Assert.Throws<DomainObjectNotFoundException>(() => action(schema_0, command));
        }

        [Theory]
        [MemberData(nameof(InvalidStates))]
        public void Commands_should_throw_exception_if_state_not_valid<T>(Action<Schema, T> action, Func<Schema, Schema> updater) where T : FieldCommand, new()
        {
            var command = new T { FieldId = 1 };

            Assert.Throws<DomainException>(() => action(updater(schema_0), command));
        }

        [Theory]
        [MemberData(nameof(InvalidNestedStates))]
        public void Commands_should_throw_exception_if_nested_state_not_valid<T>(Action<Schema, T> action, Func<Schema, Schema> updater) where T : FieldCommand, new()
        {
            var command = new T { ParentFieldId = 3, FieldId = 301 };

            Assert.Throws<DomainException>(() => action(updater(schema_0), command));
        }

        [Theory]
        [MemberData(nameof(ValidStates))]
        public void Commands_should_not_throw_exception_if_state_valid<T>(Action<Schema, T> action, Func<Schema, Schema> updater) where T : FieldCommand, new()
        {
            var command = new T { FieldId = 1 };

            action(updater(schema_0), command);
        }

        [Theory]
        [MemberData(nameof(ValidNestedStates))]
        public void Commands_should_not_throw_exception_if_nested_state_valid<T>(Action<Schema, T> action, Func<Schema, Schema> updater) where T : FieldCommand, new()
        {
            var command = new T { ParentFieldId = 3, FieldId = 301 };

            action(updater(schema_0), command);
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_locked()
        {
            var command = new DeleteField { FieldId = 1 };

            var schema_1 = schema_0.UpdateField(1, f => f.Lock());

            Assert.Throws<DomainException>(() => GuardSchemaField.CanDelete(schema_1, command));
        }

        [Fact]
        public void CanHide_should_throw_exception_if_locked()
        {
            var command = new HideField { FieldId = 1 };

            var schema_1 = schema_0.UpdateField(1, f => f.Lock());

            Assert.Throws<DomainException>(() => GuardSchemaField.CanHide(schema_1, command));
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
            var command = new UpdateField { FieldId = 1, Properties = validProperties };

            var schema_1 = schema_0.UpdateField(1, f => f.Lock());

            Assert.Throws<DomainException>(() => GuardSchemaField.CanUpdate(schema_1, command));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_not_locked()
        {
            var command = new UpdateField { FieldId = 1, Properties = validProperties };

            GuardSchemaField.CanUpdate(schema_0, command);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_properties_null()
        {
            var command = new UpdateField { FieldId = 2, Properties = null };

            ValidationAssert.Throws(() => GuardSchemaField.CanUpdate(schema_0, command),
                new ValidationError("Properties is required.", "Properties"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_properties_not_valid()
        {
            var command = new UpdateField { FieldId = 2, Properties = new StringFieldProperties { MinLength = 10, MaxLength = 5 } };

            ValidationAssert.Throws(() => GuardSchemaField.CanUpdate(schema_0, command),
                new ValidationError("Max length must be greater than min length.", "Properties.MinLength", "Properties.MaxLength"));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_field_already_exists()
        {
            var command = new AddField { Name = "field1", Properties = validProperties };

            ValidationAssert.Throws(() => GuardSchemaField.CanAdd(schema_0, command),
                new ValidationError("A field with the same name already exists."));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_nested_field_already_exists()
        {
            var command = new AddField { Name = "field301", Properties = validProperties, ParentFieldId = 3 };

            ValidationAssert.Throws(() => GuardSchemaField.CanAdd(schema_0, command),
                new ValidationError("A field with the same name already exists."));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_name_not_valid()
        {
            var command = new AddField { Name = "INVALID_NAME", Properties = validProperties };

            ValidationAssert.Throws(() => GuardSchemaField.CanAdd(schema_0, command),
                new ValidationError("Name must be a valid javascript property name.", "Name"));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_properties_not_valid()
        {
            var command = new AddField { Name = "field4", Properties = invalidProperties };

            ValidationAssert.Throws(() => GuardSchemaField.CanAdd(schema_0, command),
                new ValidationError("Max length must be greater than min length.", "Properties.MinLength", "Properties.MaxLength"));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_properties_null()
        {
            var command = new AddField { Name = "field4", Properties = null };

            ValidationAssert.Throws(() => GuardSchemaField.CanAdd(schema_0, command),
                new ValidationError("Properties is required.", "Properties"));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_partitioning_not_valid()
        {
            var command = new AddField { Name = "field4", Partitioning = "INVALID_PARTITIONING", Properties = validProperties };

            ValidationAssert.Throws(() => GuardSchemaField.CanAdd(schema_0, command),
                new ValidationError("Partitioning is not valid.", "Partitioning"));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_parent_not_exists()
        {
            var command = new AddField { Name = "field302", Properties = validProperties, ParentFieldId = 99 };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardSchemaField.CanAdd(schema_0, command));
        }

        [Fact]
        public void CanAdd_should_not_throw_exception_if_field_not_exists()
        {
            var command = new AddField { Name = "field4", Properties = validProperties };

            GuardSchemaField.CanAdd(schema_0, command);
        }

        [Fact]
        public void CanAdd_should_not_throw_exception_if_field_exists_in_root()
        {
            var command = new AddField { Name = "field1", Properties = validProperties, ParentFieldId = 3 };

            GuardSchemaField.CanAdd(schema_0, command);
        }
    }
}