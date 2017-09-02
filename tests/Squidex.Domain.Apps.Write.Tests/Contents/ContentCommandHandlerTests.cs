// ==========================================================================
//  ContentCommandMiddlewareTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentCommandMiddlewareTests : HandlerTestBase<ContentDomainObject>
    {
        private readonly ContentCommandMiddleware sut;
        private readonly ContentDomainObject content;
        private readonly ISchemaProvider schemaProvider = A.Fake<ISchemaProvider>();
        private readonly ISchemaEntity schemaEntity = A.Fake<ISchemaEntity>();
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAppEntity appEntity = A.Fake<IAppEntity>();
        private readonly NamedContentData invalidData = new NamedContentData().AddField("my-field", new ContentFieldData().SetValue(null));
        private readonly NamedContentData data = new NamedContentData().AddField("my-field", new ContentFieldData().SetValue(1));
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Create(Language.DE);
        private readonly Guid contentId = Guid.NewGuid();

        public ContentCommandMiddlewareTests()
        {
            var schema =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Invariant,
                        new NumberFieldProperties { IsRequired = true }));

            content = new ContentDomainObject(contentId, -1);

            sut = new ContentCommandMiddleware(Handler, appProvider, A.Dummy<IAssetRepository>(), schemaProvider, scriptEngine, A.Dummy<IContentRepository>());

            A.CallTo(() => appEntity.LanguagesConfig).Returns(languagesConfig);
            A.CallTo(() => appEntity.PartitionResolver).Returns(languagesConfig.ToResolver());
            A.CallTo(() => appProvider.FindAppByIdAsync(AppId)).Returns(Task.FromResult(appEntity));

            A.CallTo(() => schemaEntity.Schema).Returns(schema);
            A.CallTo(() => schemaProvider.FindSchemaByIdAsync(SchemaId, false)).Returns(Task.FromResult(schemaEntity));
        }

        [Fact]
        public async Task Create_should_throw_exception_if_data_is_not_valid()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored, true)).Returns(invalidData);

            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = invalidData });

            await TestCreate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Create_should_create_content()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored, true)).Returns(data);
            A.CallTo(() => schemaEntity.ScriptCreate).Returns("<create-script>");

            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = data });

            await TestCreate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(data, context.Result<EntityCreatedResult<NamedContentData>>().IdOrValue);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<create-script>", "create content", true)).MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_throw_exception_if_data_is_not_valid()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored, true)).Returns(invalidData);

            CreateContent();

            var context = CreateContextForCommand(new UpdateContent { ContentId = contentId, Data = invalidData });

            await TestUpdate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored, true)).Returns(data);
            A.CallTo(() => schemaEntity.ScriptUpdate).Returns("<update-script>");

            CreateContent();

            var context = CreateContextForCommand(new UpdateContent { ContentId = contentId, Data = data });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(data, context.Result<ContentDataChangedResult>().Data);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>", "update content", true)).MustHaveHappened();
        }

        [Fact]
        public async Task Patch_should_throw_exception_if_data_is_not_valid()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored, true)).Returns(invalidData);

            CreateContent();

            var context = CreateContextForCommand(new PatchContent { ContentId = contentId, Data = invalidData });

            await TestUpdate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Patch_should_update_domain_object()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored, true)).Returns(data);
            A.CallTo(() => schemaEntity.ScriptUpdate).Returns("<update-script>");

            var path = new NamedContentData().AddField("my-field", new ContentFieldData().SetValue(3));

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored, true)).Returns(path);

            CreateContent();

            var context = CreateContextForCommand(new PatchContent { ContentId = contentId, Data = path });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.NotNull(context.Result<ContentDataChangedResult>().Data);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>", "patch content", true)).MustHaveHappened();
        }

        [Fact]
        public async Task Publish_should_publish_domain_object()
        {
            A.CallTo(() => schemaEntity.ScriptPublish).Returns("<publish-script>");

            CreateContent();

            var context = CreateContextForCommand(new PublishContent { ContentId = contentId });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<publish-script>", "publish content")).MustHaveHappened();
        }

        [Fact]
        public async Task Unpublish_should_unpublish_domain_object()
        {
            A.CallTo(() => schemaEntity.ScriptUnpublish).Returns("<unpublish-script>");

            CreateContent();

            var context = CreateContextForCommand(new UnpublishContent { ContentId = contentId });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<unpublish-script>", "unpublish content")).MustHaveHappened();
        }

        [Fact]
        public async Task Delete_should_update_domain_object()
        {
            A.CallTo(() => schemaEntity.ScriptDelete).Returns("<delete-script>");

            CreateContent();

            var command = CreateContextForCommand(new DeleteContent { ContentId = contentId });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(command);
            });

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<delete-script>", "delete content")).MustHaveHappened();
        }

        private void CreateContent()
        {
            content.Create(new CreateContent { Data = data });
        }
    }
}
