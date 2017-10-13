// ==========================================================================
//  SchemaFieldGuardTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Schemas.Guards
{
    public class SchemaFieldGuardTests
    {
        private Schema schema =
            Schema.Create("my-schema", new SchemaProperties())
                .AddField(new StringField(1, "field1", Partitioning.Invariant))
                .AddField(new StringField(2, "field2", Partitioning.Invariant));

        [Fact]
        public void Should_throw_exception_if_field_to_hide_already_hidden()
        {
            schema = schema.HideField(1);

            Assert.Throws<DomainException>(() => SchemaFieldGuard.GuardCanHide(schema, 1));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_hide_not_found()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => SchemaFieldGuard.GuardCanHide(schema, 3));
        }

        [Fact]
        public void Should_not_throw_exception_if_field_to_hide_shown()
        {
            SchemaFieldGuard.GuardCanHide(schema, 1);
        }

        [Fact]
        public void Should_throw_exception_if_field_to_disable_not_found()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => SchemaFieldGuard.GuardCanDisable(schema, 3));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_disable_already_disabled()
        {
            schema = schema.DisableField(1);

            Assert.Throws<DomainException>(() => SchemaFieldGuard.GuardCanDisable(schema, 1));
        }

        [Fact]
        public void Should_not_throw_exception_if_field_to_disable_shown()
        {
            SchemaFieldGuard.GuardCanDisable(schema, 1);
        }

        [Fact]
        public void Should_throw_exception_if_field_to_show_already_shown()
        {
            Assert.Throws<DomainException>(() => SchemaFieldGuard.GuardCanShow(schema, 1));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_show_not_found()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => SchemaFieldGuard.GuardCanShow(schema, 3));
        }

        [Fact]
        public void Should_not_throw_exception_if_field_to_show_hidden()
        {
            schema = schema.HideField(1);

            SchemaFieldGuard.GuardCanShow(schema, 1);
        }

        [Fact]
        public void Should_throw_exception_if_field_to_enable_already_enabled()
        {
            Assert.Throws<DomainException>(() => SchemaFieldGuard.GuardCanEnable(schema, 1));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_enable_not_found()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => SchemaFieldGuard.GuardCanEnable(schema, 3));
        }

        [Fact]
        public void Should_not_throw_exception_if_field_to_enable_disabled()
        {
            schema = schema.DisableField(1);

            SchemaFieldGuard.GuardCanEnable(schema, 1);
        }

        [Fact]
        public void Should_throw_exception_if_field_to_lock_already_locked()
        {
            schema = schema.LockField(1);

            Assert.Throws<DomainException>(() => SchemaFieldGuard.GuardCanLock(schema, 1));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_lock_not_found()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => SchemaFieldGuard.GuardCanLock(schema, 3));
        }

        [Fact]
        public void Should_not_throw_exception_if_field_to_lock_not_locked()
        {
            SchemaFieldGuard.GuardCanLock(schema, 1);
        }

        [Fact]
        public void Should_throw_exception_if_field_to_delete_not_found()
        {
            Assert.Throws<DomainObjectNotFoundException>(() => SchemaFieldGuard.GuardCanDelete(schema, 3));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_delete_is_locked()
        {
            schema = schema.LockField(1);

            Assert.Throws<DomainException>(() => SchemaFieldGuard.GuardCanDelete(schema, 1));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_update_not_locked()
        {
           SchemaFieldGuard.GuardCanUpdate(schema, 1);
        }

        [Fact]
        public void Should_throw_exception_if_field_to_update_is_locked()
        {
            schema = schema.LockField(1);

            Assert.Throws<DomainException>(() => SchemaFieldGuard.GuardCanUpdate(schema, 1));
        }

        [Fact]
        public void Should_throw_exception_if_field_to_delete_not_locked()
        {
            SchemaFieldGuard.GuardCanDelete(schema, 1);
        }

        [Fact]
        public void Should_throw_exception_if_field_to_add_already_exists()
        {
            Assert.Throws<ValidationException>(() => SchemaFieldGuard.GuardCanAdd(schema, "field1"));
        }

        [Fact]
        public void Should_not_throw_exception_if_field_to_add_not_exists()
        {
            SchemaFieldGuard.GuardCanAdd(schema, "field3");
        }
    }
}
