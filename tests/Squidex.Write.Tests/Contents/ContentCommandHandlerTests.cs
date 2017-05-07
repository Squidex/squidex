// ==========================================================================
//  ContentCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Moq;
using Squidex.Core;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Services;
using Squidex.Read.Schemas;
using Squidex.Read.Schemas.Services;
using Squidex.Write.Contents.Commands;
using Squidex.Write.TestHelpers;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Contents
{
    public class ContentCommandHandlerTests : HandlerTestBase<ContentDomainObject>
    {
        private readonly ContentCommandHandler sut;
        private readonly ContentDomainObject content;
        private readonly Mock<ISchemaProvider> schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IAppProvider> appProvider = new Mock<IAppProvider>();
        private readonly Mock<ISchemaEntityWithSchema> schemaEntity = new Mock<ISchemaEntityWithSchema>();
        private readonly Mock<IAppEntity> appEntity = new Mock<IAppEntity>();
        private readonly ContentData data = new ContentData().AddField("my-field", new ContentFieldData().SetValue(1));
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Create(Language.DE);
        private readonly Guid contentId = Guid.NewGuid();

        public ContentCommandHandlerTests()
        {
            var schema =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new NumberField(1, "my-field",
                        new NumberFieldProperties { IsRequired = true }));

            content = new ContentDomainObject(contentId, -1);

            sut = new ContentCommandHandler(Handler, appProvider.Object, schemaProvider.Object);

            appEntity.Setup(x => x.LanguagesConfig).Returns(languagesConfig);
            appProvider.Setup(x => x.FindAppByIdAsync(AppId)).Returns(Task.FromResult(appEntity.Object));

            schemaEntity.Setup(x => x.Schema).Returns(schema);
            schemaProvider.Setup(x => x.FindSchemaByIdAsync(SchemaId, false)).Returns(Task.FromResult(schemaEntity.Object));
        }

        [Fact]
        public async Task Create_should_throw_exception_if_data_is_not_valid()
        {
            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = new ContentData() });

            await TestCreate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Create_should_create_content()
        {
            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = data });

            await TestCreate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(data, context.Result<EntityCreatedResult<ContentData>>().IdOrValue);
        }

        [Fact]
        public async Task Update_should_throw_exception_if_data_is_not_valid()
        {
            CreateContent();

            var context = CreateContextForCommand(new UpdateContent { ContentId = contentId, Data = new ContentData() });

            await TestUpdate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            CreateContent();

            var context = CreateContextForCommand(new UpdateContent { ContentId = contentId, Data = data });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Patch_should_throw_exception_if_data_is_not_valid()
        {
            CreateContent();

            var context = CreateContextForCommand(new PatchContent { ContentId = contentId, Data = new ContentData() });

            await TestUpdate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Patch_should_update_domain_object()
        {
            var otherContent = new ContentData().AddField("my-field", new ContentFieldData().SetValue(3));

            CreateContent();

            var context = CreateContextForCommand(new PatchContent { ContentId = contentId, Data = otherContent });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Publish_should_publish_domain_object()
        {
            CreateContent();

            var context = CreateContextForCommand(new PublishContent { ContentId = contentId });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Unpublish_should_unpublish_domain_object()
        {
            CreateContent();

            var context = CreateContextForCommand(new UnpublishContent { ContentId = contentId });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Delete_should_update_domain_object()
        {
            CreateContent();

            var command = CreateContextForCommand(new DeleteContent { ContentId = contentId });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(command);
            });
        }

        private void CreateContent()
        {
            content.Create(new CreateContent { Data = data });
        }
    }
}
