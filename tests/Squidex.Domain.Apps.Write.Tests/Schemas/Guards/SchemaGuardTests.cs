// ==========================================================================
//  SchemaGuardTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Schemas.Guards
{
    public class SchemaGuardTests
    {
        private Schema schema =
            Schema.Create("my-schema", new SchemaProperties())
                .AddField(new StringField(1, "field1", Partitioning.Invariant))
                .AddField(new StringField(2, "field2", Partitioning.Invariant));

        [Fact]
        public void Should_throw_exception_if_schema_to_publish_already_published()
        {
            schema = schema.Publish();

            Assert.Throws<DomainException>(() => SchemaGuard.GuardCanPublish(schema));
        }

        [Fact]
        public void Should_not_throw_exception_if_schema_to_publish_not_published()
        {
            SchemaGuard.GuardCanPublish(schema);
        }

        [Fact]
        public void Should_throw_exception_if_schema_to_unpublish_already_unpublished()
        {
            Assert.Throws<DomainException>(() => SchemaGuard.GuardCanUnpublish(schema));
        }

        [Fact]
        public void Should_not_throw_exception_if_schema_to_unpublish_published()
        {
            schema = schema.Publish();

            SchemaGuard.GuardCanUnpublish(schema);
        }

        [Fact]
        public void Should_throw_excepotion_if_schema_fields_to_reorder_not_valid()
        {
            Assert.Throws<ValidationException>(() => SchemaGuard.GuardCanReorder(schema, new List<long> { 1 }));
            Assert.Throws<ValidationException>(() => SchemaGuard.GuardCanReorder(schema, new List<long> { 1, 3 }));
        }

        [Fact]
        public void Should_not_throw_excepotion_if_schema_fields_to_reorder_are_valid()
        {
            SchemaGuard.GuardCanReorder(schema, new List<long> { 1, 2 });
        }
    }
}
