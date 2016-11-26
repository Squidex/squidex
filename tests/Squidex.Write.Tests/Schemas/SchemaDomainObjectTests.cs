// ==========================================================================
//  SchemaDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using FluentAssertions;
using Squidex.Core.Schemas;
using Squidex.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Write.Schemas;
using Squidex.Write.Schemas.Commands;
using Xunit;

namespace Squidex.Write.Tests.Schemas
{
    [Collection("Schema")]
    public class SchemaDomainObjectTests
    {
        private const string TestName = "schema";
        private readonly Guid appId = Guid.NewGuid();
        private readonly FieldRegistry registry = new FieldRegistry();
        private readonly SchemaDomainObject sut;

        public SchemaDomainObjectTests()
        {
            sut = new SchemaDomainObject(Guid.NewGuid(), 0, registry);
        }

        [Fact]
        public void Create_should_throw_if_created()
        {
            sut.Create(new CreateSchema { Name = TestName });

            Assert.Throws<DomainException>(() => sut.Create(new CreateSchema { Name = TestName }));
        }

        [Fact]
        public void Create_should_throw_if_command_is_invalid()
        {
            Assert.Throws<ValidationException>(() => sut.Create(new CreateSchema()));
        }

        [Fact]
        public void Create_should_create_schema()
        {
            var properties = new SchemaProperties();

            sut.Create(new CreateSchema { Name = TestName, AppId = appId, Properties = properties });
            
            Assert.Equal("schema", sut.Schema.Name);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new SchemaCreated { Name = TestName, AppId = appId, Properties = properties }
                    });
        }

        [Fact]
        public void Update_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.Update(new UpdateSchema { Properties = new SchemaProperties() }));
        }

        [Fact]
        public void Update_should_throw_if_schema_is_deleted()
        {
            sut.Create(new CreateSchema { Name = TestName });
            sut.Delete();

            Assert.Throws<ValidationException>(() => sut.Update(new UpdateSchema()));
        }

        [Fact]
        public void Update_should_throw_if_command_is_invalid()
        {
            sut.Create(new CreateSchema { Name = TestName });

            Assert.Throws<ValidationException>(() => sut.Update(new UpdateSchema()));
        }

        [Fact]
        public void Update_should_refresh_properties()
        {
            var properties = new SchemaProperties();

            sut.Create(new CreateSchema { Name = TestName, AppId = appId });
            sut.Update(new UpdateSchema { Properties = properties });
            
            Assert.Equal(properties, sut.Schema.Properties);

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new SchemaUpdated { Properties = properties }
                    });
        }

        [Fact]
        public void Delete_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.Delete());
        }

        [Fact]
        public void Delete_should_throw_if_already_deleted()
        {
            sut.Create(new CreateSchema { Name = TestName });
            sut.Delete();

            Assert.Throws<DomainException>(() => sut.Delete());
        }

        [Fact]
        public void Delete_should_refresh_properties()
        {
            sut.Create(new CreateSchema { Name = TestName, AppId = appId });
            sut.Delete();

            Assert.True(sut.IsDeleted);

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new SchemaDeleted()
                    });
        }
    }
}
