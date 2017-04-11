// ==========================================================================
//  SchemaCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Moq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Read.Schemas;
using Squidex.Read.Schemas.Services;
using Squidex.Write.Schemas.Commands;
using Squidex.Write.TestHelpers;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Schemas
{
    public class SchemaCommandHandlerTests : HandlerTestBase<SchemaDomainObject>
    {
        private readonly Mock<ISchemaProvider> schemaProvider = new Mock<ISchemaProvider>();
        private readonly SchemaCommandHandler sut;
        private readonly SchemaDomainObject schema;
        private readonly FieldRegistry registry = new FieldRegistry(new TypeNameRegistry());
        private readonly string fieldName = "age";

        public SchemaCommandHandlerTests()
        {
            schema = new SchemaDomainObject(SchemaId, -1, registry);

            sut = new SchemaCommandHandler(Handler, schemaProvider.Object);
        }

        [Fact]
        public async Task Create_should_throw_if_a_name_with_same_name_already_exists()
        {
            var context = CreateContextForCommand(new CreateSchema { Name = SchemaName, SchemaId = SchemaId });

            schemaProvider.Setup(x => x.FindSchemaByNameAsync(AppId, SchemaName))
                .Returns(Task.FromResult(new Mock<ISchemaEntityWithSchema>().Object))
                .Verifiable();

            await TestCreate(schema, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(async () => await sut.HandleAsync(context));
            }, false);

            schemaProvider.VerifyAll();
        }

        [Fact]
        public async Task Create_should_create_schema_if_name_is_free()
        {
            var context = CreateContextForCommand(new CreateSchema { Name = SchemaName, SchemaId = SchemaId });

            schemaProvider.Setup(x => x.FindSchemaByNameAsync(AppId, SchemaName))
                .Returns(Task.FromResult<ISchemaEntityWithSchema>(null))
                .Verifiable();

            await TestCreate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(SchemaId, context.Result<EntityCreatedResult<Guid>>().IdOrValue);
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
