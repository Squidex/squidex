// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaCommandMiddlewareTests : HandlerTestBase<SchemaDomainObject>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly SchemaCommandMiddleware sut;
        private readonly SchemaDomainObject schema;
        private readonly FieldRegistry registry = new FieldRegistry(new TypeNameRegistry());
        private readonly string fieldName = "age";

        protected override Guid Id
        {
            get { return SchemaId; }
        }

        public SchemaCommandMiddlewareTests()
        {
            schema = new SchemaDomainObject(registry);

            sut = new SchemaCommandMiddleware(Handler, appProvider);

            A.CallTo(() => appProvider.GetSchemaAsync(AppId, SchemaName))
                .Returns((ISchemaEntity)null);
        }

        [Fact]
        public async Task Create_should_create_schema_domain_object()
        {
            var context = CreateContextForCommand(new CreateSchema { Name = SchemaName, SchemaId = SchemaId });

            await TestCreate(schema, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(SchemaId, context.Result<EntityCreatedResult<Guid>>().IdOrValue);

            A.CallTo(() => appProvider.GetSchemaAsync(AppId, SchemaName)).MustHaveHappened();
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
        public async Task ConfigureScripts_should_update_domain_object()
        {
            CreateSchema();

            var context = CreateContextForCommand(new ConfigureScripts());

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
        public async Task Add_should_update_domain_object()
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
        public async Task LockField_should_update_domain_object()
        {
            CreateSchema();
            CreateField();

            var context = CreateContextForCommand(new LockField { FieldId = 1 });

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

            HideField();

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

            DisableField();

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
            schema.Add(CreateCommand(new AddField { Name = fieldName, Properties = new NumberFieldProperties() }));
        }

        private void HideField()
        {
            schema.HideField(CreateCommand(new HideField { FieldId = 1 }));
        }

        private void DisableField()
        {
            schema.DisableField(CreateCommand(new DisableField { FieldId = 1 }));
        }
    }
}