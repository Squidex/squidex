// ==========================================================================
//  SchemaDomainObjectTest.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using FluentAssertions;
using PinkParrot.Core.Schemas;
using PinkParrot.Events.Schemas;
using PinkParrot.Infrastructure;
using PinkParrot.Write.Schemas;
using PinkParrot.Write.Schemas.Commands;
using Xunit;

namespace PinkParrot.Write.Tests.Schemas
{
    [Collection("Schema")]
    public class SchemaDomainObjectTest
    {
        private const string TestName = "schema";
        private readonly Guid appId = Guid.NewGuid();
        private readonly FieldRegistry registry = new FieldRegistry();
        private readonly SchemaDomainObject sut;

        public SchemaDomainObjectTest()
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
            var props = new SchemaProperties(null, null);

            sut.Create(new CreateSchema { Name = TestName, AppId = appId, Properties = props });
            
            Assert.Equal("schema", sut.Schema.Name);
            Assert.Equal(props, sut.Schema.Properties);
            Assert.Equal(appId, sut.AppId);

            sut.GetUncomittedEvents()
                .Select(x => x.Payload as SchemaCreated)
                .Single()
                .ShouldBeEquivalentTo(
                    new SchemaCreated { Name = TestName, AppId = appId, Properties = props });
        }

        [Fact]
        public void Update_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.Update(new UpdateSchema { Properties = new SchemaProperties(null, null) }));
        }

        [Fact]
        public void Update_should_throw_if_command_is_deleted()
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
            var props = new SchemaProperties(null, null);

            sut.Create(new CreateSchema { Name = TestName, AppId = appId });
            sut.Update(new UpdateSchema { Properties = props });
            
            Assert.Equal(props, sut.Schema.Properties);

            sut.GetUncomittedEvents()
                .Select(x => x.Payload as SchemaUpdated)
                .Last()
                .ShouldBeEquivalentTo(
                    new SchemaUpdated { Properties = props });
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
            Assert.IsType<SchemaDeleted>(sut.GetUncomittedEvents().Last().Payload);
        }
    }
}
