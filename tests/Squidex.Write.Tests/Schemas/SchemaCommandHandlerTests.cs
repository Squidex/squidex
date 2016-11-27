// ==========================================================================
//  SchemaCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Read.Schemas.Services;
using Squidex.Write.Schemas;
using Squidex.Write.Schemas.Commands;
using Squidex.Write.Tests.Utils;
using Xunit;
// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Tests.Schemas
{
    public class SchemaCommandHandlerTests : HandlerTestBase<SchemaDomainObject>
    {
        private readonly Mock<ISchemaProvider> schemaProvider = new Mock<ISchemaProvider>();
        private readonly SchemaCommandHandler sut;
        private readonly SchemaDomainObject schema;
        private readonly FieldRegistry registry = new FieldRegistry();
        private readonly Guid appId = Guid.NewGuid();
        private readonly string fieldName = "age";
        private readonly string schemaName = "users";

        static SchemaCommandHandlerTests()
        {
            TypeNameRegistry.Map(typeof(Schema).GetTypeInfo().Assembly);
        }

        public SchemaCommandHandlerTests()
        {
            schema = new SchemaDomainObject(Id, 0, registry);

            sut = new SchemaCommandHandler(
                DomainObjectFactory.Object,
                DomainObjectRepository.Object,
                schemaProvider.Object);
        }

        [Fact]
        public async Task Create_should_throw_if_a_name_with_same_name_already_exists()
        {
            var command = new CreateSchema { Name = schemaName, AppId = appId, AggregateId = Id };
            var context = new CommandContext(command);

            schemaProvider.Setup(x => x.FindSchemaIdByNameAsync(appId, schemaName)).Returns(Task.FromResult<Guid?>(Guid.NewGuid())).Verifiable();

            await TestCreate(schema, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(async () => await sut.HandleAsync(context));
            }, false);

            schemaProvider.VerifyAll();
        }

        [Fact]
        public async Task Create_should_create_schema_if_name_is_free()
        {
            var command = new CreateSchema { Name = schemaName, AppId = appId, AggregateId = Id };
            var context = new CommandContext(command);

            schemaProvider.Setup(x => x.FindSchemaIdByNameAsync(Id, schemaName)).Returns(Task.FromResult<Guid?>(null)).Verifiable();

            await TestCreate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(command.Name, context.Result<string>());
        }

        [Fact]
        public async Task UpdateSchema_should_update_domain_object()
        {
            CreateSchema();

            var command = new UpdateSchema { AggregateId = Id, Properties = new SchemaProperties() };
            var context = new CommandContext(command);

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task DeleteSchema_should_update_domain_object()
        {
            CreateSchema();

            var command = new DeleteSchema { AggregateId = Id };
            var context = new CommandContext(command);

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task AddField_should_update_domain_object()
        {
            CreateSchema();

            var command = new AddField { AggregateId = Id, Name = fieldName, Properties = new NumberFieldProperties() };
            var context = new CommandContext(command);

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(1, context.Result<long>());
        }

        [Fact]
        public async Task UpdateField_should_update_domain_object()
        {
            CreateSchema();
            CreateField();

            var command = new UpdateField { AggregateId = Id, FieldId = 1, Properties = new NumberFieldProperties() };
            var context = new CommandContext(command);

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

            var command = new HideField { AggregateId = Id, FieldId = 1 };
            var context = new CommandContext(command);

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

            var command = new ShowField { AggregateId = Id, FieldId = 1 };
            var context = new CommandContext(command);

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

            var command = new DisableField { AggregateId = Id, FieldId = 1 };
            var context = new CommandContext(command);

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

            var command = new EnableField { AggregateId = Id, FieldId = 1 };
            var context = new CommandContext(command);

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

            var command = new DeleteField { AggregateId = Id, FieldId = 1 };
            var context = new CommandContext(command);

            await TestUpdate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        private void CreateSchema()
        {
            schema.Create(new CreateSchema { Name = schemaName });
        }

        private void CreateField()
        {
            schema.AddField(new AddField { Name = fieldName, Properties = new NumberFieldProperties() });
        }
    }
}
