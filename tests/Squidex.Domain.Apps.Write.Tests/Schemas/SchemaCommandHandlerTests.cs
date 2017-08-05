// ==========================================================================
//  SchemaCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Schemas
{
    public class SchemaCommandHandlerTests : HandlerTestBase<SchemaDomainObject>
    {
        private readonly ISchemaProvider schemaProvider = A.Fake<ISchemaProvider>();
        private readonly SchemaCommandHandler sut;
        private readonly SchemaDomainObject schema;
        private readonly FieldRegistry registry = new FieldRegistry(new TypeNameRegistry());
        private readonly string fieldName = "age";

        public SchemaCommandHandlerTests()
        {
            schema = new SchemaDomainObject(SchemaId, -1, registry);

            sut = new SchemaCommandHandler(Handler, schemaProvider);
        }

        [Fact]
        public async Task Create_should_throw_exception_if_a_name_with_same_name_already_exists()
        {
            var context = CreateContextForCommand(new CreateSchema { Name = SchemaName, SchemaId = SchemaId });

            A.CallTo(() => schemaProvider.FindSchemaByNameAsync(AppId, SchemaName))
                .Returns(Task.FromResult(A.Dummy<ISchemaEntity>()));

            await TestCreate(schema, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(async () => await sut.HandleAsync(context));
            }, false);

            A.CallTo(() => schemaProvider.FindSchemaByNameAsync(AppId, SchemaName)).MustHaveHappened();
        }

        [Fact]
        public async Task Create_should_create_schema_if_name_is_free()
        {
            var context = CreateContextForCommand(new CreateSchema { Name = SchemaName, SchemaId = SchemaId });

            A.CallTo(() => schemaProvider.FindSchemaByNameAsync(AppId, SchemaName))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            await TestCreate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(SchemaId, context.Result<EntityCreatedResult<Guid>>().IdOrValue);

            A.CallTo(() => schemaProvider.FindSchemaByNameAsync(AppId, SchemaName)).MustHaveHappened();
        }

        [Fact]
        public async Task UpdateSchema_should_update_domain_object()
        {
            CreateSchema();

            var context = CreateContextForCommand(new UpdateSchema { Properties = new SchemaProperties() });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task ReorderSchema_should_update_domain_object()
        {
            CreateSchema();

            var context = CreateContextForCommand(new ReorderFields { FieldIds = new List<long>() });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task PublishSchema_should_update_domain_object()
        {
            CreateSchema();

            var context = CreateContextForCommand(new PublishSchema());

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task UnpublishSchema_should_update_domain_object()
        {
            CreateSchema();
            PublishSchema();

            var context = CreateContextForCommand(new UnpublishSchema());

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task DeleteSchema_should_update_domain_object()
        {
            CreateSchema();

            var context = CreateContextForCommand(new DeleteSchema());

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task AddField_should_update_domain_object()
        {
            CreateSchema();

            var context = CreateContextForCommand(new AddField { Name = fieldName, Properties = new NumberFieldProperties() });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(1, context.Result<EntityCreatedResult<long>>().IdOrValue);
        }

        [Fact]
        public async Task UpdateField_should_update_domain_object()
        {
            CreateSchema();
            CreateField();

            var context = CreateContextForCommand(new UpdateField { FieldId = 1, Properties = new NumberFieldProperties() });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task HideField_should_update_domain_object()
        {
            CreateSchema();
            CreateField();

            var context = CreateContextForCommand(new HideField { FieldId = 1 });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task ShowField_should_update_domain_object()
        {
            CreateSchema();
            CreateField();

            var context = CreateContextForCommand(new ShowField { FieldId = 1 });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task DisableField_should_update_domain_object()
        {
            CreateSchema();
            CreateField();

            var context = CreateContextForCommand(new DisableField { FieldId = 1 });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task EnableField_should_update_domain_object()
        {
            CreateSchema();
            CreateField();

            var context = CreateContextForCommand(new EnableField { FieldId = 1 });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task DeleteField_should_update_domain_object()
        {
            CreateSchema();
            CreateField();

            var context = CreateContextForCommand(new DeleteField { FieldId = 1 });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task AddWebhook_should_update_domain_object()
        {
            CreateSchema();

            var context = CreateContextForCommand(new AddWebhook { Url = new Uri("http://cloud.squidex.io") });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task DeleteWebhook_should_update_domain_object()
        {
            var createCommand = new AddWebhook { Url = new Uri("http://cloud.squidex.io") };

            CreateSchema();
            CreateWebhook(createCommand);

            var context = CreateContextForCommand(new DeleteWebhook { Id = createCommand.Id });

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        private void CreateWebhook(AddWebhook command)
        {
            schema.AddWebhook(command);
        }

        private void CreateSchema()
        {
            schema.Create(CreateCommand(new CreateSchema { Name = SchemaName }));
        }

        private void PublishSchema()
        {
            schema.Publish(CreateCommand(new PublishSchema()));
        }

        private void CreateField()
        {
            schema.AddField(CreateCommand(new AddField { Name = fieldName, Properties = new NumberFieldProperties() }));
        }
    }
}
