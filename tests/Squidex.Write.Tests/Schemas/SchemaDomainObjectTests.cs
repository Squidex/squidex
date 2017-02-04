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
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Write.Schemas.Commands;
using Xunit;
// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Schemas
{
    public class SchemaDomainObjectTests
    {
        private readonly Guid appId = Guid.NewGuid();
        private readonly string fieldName = "age";
        private readonly string appName = "schema";
        private readonly FieldRegistry registry = new FieldRegistry(new TypeNameRegistry());
        private readonly SchemaDomainObject sut;
        
        public SchemaDomainObjectTests()
        {
            sut = new SchemaDomainObject(Guid.NewGuid(), 0, registry);
        }

        [Fact]
        public void Create_should_throw_if_created()
        {
            sut.Create(new CreateSchema { Name = appName });

            Assert.Throws<DomainException>(() => sut.Create(new CreateSchema { Name = appName }));
        }

        [Fact]
        public void Create_should_throw_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() => sut.Create(new CreateSchema()));
        }

        [Fact]
        public void Create_should_create_schema_and_create_events()
        {
            var properties = new SchemaProperties();

            sut.Create(new CreateSchema { Name = appName, AppId = appId, Properties = properties });
            
            Assert.Equal("schema", sut.Schema.Name);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new SchemaCreated { Name = appName, Properties = properties }
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
            CreateSchema();
            DeleteSchema();

            Assert.Throws<ValidationException>(() => sut.Update(new UpdateSchema()));
        }

        [Fact]
        public void Update_should_throw_if_command_is_not_valid()
        {
            CreateSchema();

            Assert.Throws<ValidationException>(() => sut.Update(new UpdateSchema()));
        }

        [Fact]
        public void Update_should_refresh_properties_and_create_events()
        {
            var properties = new SchemaProperties();

            CreateSchema();

            sut.Update(new UpdateSchema { Properties = properties });
            
            Assert.Equal(properties, sut.Schema.Properties);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new SchemaUpdated { Properties = properties }
                    });
        }

        [Fact]
        public void Publish_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.Publish(new PublishSchema()));
        }

        [Fact]
        public void Publish_should_throw_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.Publish(new PublishSchema()));
        }

        [Fact]
        public void Publish_should_refresh_properties_and_create_events()
        {
            CreateSchema();

            sut.Publish(new PublishSchema());

            Assert.True(sut.Schema.IsPublished);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new SchemaPublished()
                    });
        }

        [Fact]
        public void Unpublish_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.Unpublish(new UnpublishSchema()));
        }

        [Fact]
        public void Unpublish_should_throw_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.Unpublish(new UnpublishSchema()));
        }

        [Fact]
        public void Unpublish_should_refresh_properties_and_create_events()
        {
            CreateSchema();
            PublishSchema();

            sut.Unpublish(new UnpublishSchema());

            Assert.False(sut.Schema.IsPublished);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new SchemaUnpublished()
                    });
        }

        [Fact]
        public void Delete_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.Delete(new DeleteSchema()));
        }

        [Fact]
        public void Delete_should_throw_if_already_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.Delete(new DeleteSchema()));
        }

        [Fact]
        public void Delete_should_refresh_properties_and_create_events()
        {
            CreateSchema();

            sut.Delete(new DeleteSchema());

            Assert.True(sut.IsDeleted);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new SchemaDeleted()
                    });
        }

        [Fact]
        public void AddField_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.AddField(new AddField { Name = fieldName, Properties = new NumberFieldProperties() }));
        }

        [Fact]
        public void AddField_should_throw_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() => sut.AddField(new AddField()));
        }

        [Fact]
        public void AddField_should_throw_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.AddField(new AddField { Name = fieldName, Properties = new NumberFieldProperties() }));
        }

        [Fact]
        public void AddField_should_update_schema_and_create_events()
        {
            var properties = new NumberFieldProperties();

            CreateSchema();

            sut.AddField(new AddField { Name = fieldName, Properties = properties });

            Assert.Equal(properties, sut.Schema.Fields[1].RawProperties);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new FieldAdded { Name = fieldName, FieldId = 1, Properties = properties }
                    });
        }

        [Fact]
        public void UpdateField_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.UpdateField(new UpdateField { FieldId = 1, Properties = new NumberFieldProperties() }));
        }

        [Fact]
        public void UpdateField_should_throw_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() => sut.UpdateField(new UpdateField()));
        }

        [Fact]
        public void UpdateField_should_throw_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.UpdateField(new UpdateField { FieldId = 1, Properties = new NumberFieldProperties() }));
        }

        [Fact]
        public void UpdateField_should_throw_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.UpdateField(new UpdateField { FieldId = 1, Properties = new NumberFieldProperties() }));
        }

        [Fact]
        public void UpdateField_should_update_schema_and_create_events()
        {
            var properties = new NumberFieldProperties();

            CreateSchema();
            CreateField();

            sut.UpdateField(new UpdateField { FieldId = 1, Properties = properties });

            Assert.Equal(properties, sut.Schema.Fields[1].RawProperties);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new FieldUpdated { FieldId = 1, Properties = properties }
                    });
        }

        [Fact]
        public void HideField_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.HideField(new HideField { FieldId = 1 }));
        }

        [Fact]
        public void HideField_should_throw_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.HideField(new HideField { FieldId = 2 }));
        }

        [Fact]
        public void HideField_should_throw_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.HideField(new HideField { FieldId = 1 }));
        }

        [Fact]
        public void HideField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.HideField(new HideField { FieldId = 1 });

            Assert.True(sut.Schema.Fields[1].IsHidden);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new FieldHidden { FieldId = 1 }
                    });
        }

        [Fact]
        public void ShowField_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.ShowField(new ShowField { FieldId = 1 }));
        }

        [Fact]
        public void ShowField_should_throw_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.ShowField(new ShowField { FieldId = 2 }));
        }

        [Fact]
        public void ShowField_should_throw_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.ShowField(new ShowField { FieldId = 1 }));
        }

        [Fact]
        public void ShowField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.HideField(new HideField { FieldId = 1 });
            sut.ShowField(new ShowField { FieldId = 1 });

            Assert.False(sut.Schema.Fields[1].IsHidden);

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new FieldShown { FieldId = 1 }
                    });
        }

        [Fact]
        public void DisableField_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.DisableField(new DisableField { FieldId = 1 }));
        }

        [Fact]
        public void DisableField_should_throw_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.DisableField(new DisableField { FieldId = 2 }));
        }

        [Fact]
        public void DisableField_should_throw_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.DisableField(new DisableField { FieldId = 1 }));
        }

        [Fact]
        public void DisableField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();
            
            sut.DisableField(new DisableField { FieldId = 1 });

            Assert.True(sut.Schema.Fields[1].IsDisabled);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new FieldDisabled { FieldId = 1 }
                    });
        }

        [Fact]
        public void EnableField_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.EnableField(new EnableField { FieldId = 1 }));
        }

        [Fact]
        public void EnableField_should_throw_if_field_is_not_found()
        {
            CreateSchema();

            Assert.Throws<DomainObjectNotFoundException>(() => sut.EnableField(new EnableField { FieldId = 2 }));
        }

        [Fact]
        public void EnableField_should_throw_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.EnableField(new EnableField { FieldId = 1 }));
        }

        [Fact]
        public void EnableField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.DisableField(new DisableField { FieldId = 1 });
            sut.EnableField(new EnableField { FieldId = 1 });

            Assert.False(sut.Schema.Fields[1].IsDisabled);

            sut.GetUncomittedEvents().Select(x => x.Payload).Skip(1).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new FieldEnabled { FieldId = 1 }
                    });
        }

        [Fact]
        public void DeleteField_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.DeleteField(new DeleteField { FieldId = 1 }));
        }

        [Fact]
        public void DeleteField_should_throw_if_schema_is_deleted()
        {
            CreateSchema();
            DeleteSchema();

            Assert.Throws<DomainException>(() => sut.DeleteField(new DeleteField { FieldId = 1 }));
        }

        [Fact]
        public void DeleteField_should_update_schema_and_create_events()
        {
            CreateSchema();
            CreateField();

            sut.DeleteField(new DeleteField { FieldId = 1 });

            Assert.False(sut.Schema.Fields.ContainsKey(1));

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new FieldDeleted { FieldId = 1 }
                    });
        }

        private void CreateField()
        {
            sut.AddField(new AddField { Name = fieldName, Properties = new NumberFieldProperties() });

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void CreateSchema()
        {
            sut.Create(new CreateSchema { Name = appName, AppId = appId });

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void PublishSchema()
        {
            sut.Publish(new PublishSchema());

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void DeleteSchema()
        {
            sut.Delete(new DeleteSchema());

            ((IAggregate)sut).ClearUncommittedEvents();
        }
    }
}
