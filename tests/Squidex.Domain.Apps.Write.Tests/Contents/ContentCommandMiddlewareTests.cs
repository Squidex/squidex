// ==========================================================================
//  ContentCommandMiddlewareTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Security.Claims;
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

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentCommandMiddlewareTests : HandlerTestBase<ContentDomainObject>
    {
        private readonly ContentCommandMiddleware sut;
        private readonly ContentDomainObject content;
        private readonly ISchemaProvider schemas = A.Fake<ISchemaProvider>();
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly ClaimsPrincipal user = new ClaimsPrincipal();
        private readonly NamedContentData invalidData = new NamedContentData().AddField("my-field", new ContentFieldData().SetValue(null));
        private readonly NamedContentData data = new NamedContentData().AddField("my-field", new ContentFieldData().SetValue(1));
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Create(Language.DE);
        private readonly Guid contentId = Guid.NewGuid();

        public ContentCommandMiddlewareTests()
        {
            var schemaDef =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Invariant,
                        new NumberFieldProperties { IsRequired = true }));

            content = new ContentDomainObject(contentId, -1);

            sut = new ContentCommandMiddleware(Handler, appProvider, A.Dummy<IAssetRepository>(), schemas, scriptEngine, A.Dummy<IContentRepository>());

            A.CallTo(() => app.LanguagesConfig).Returns(languagesConfig);
            A.CallTo(() => app.PartitionResolver).Returns(languagesConfig.ToResolver());
            A.CallTo(() => appProvider.FindAppByIdAsync(AppId)).Returns(app);

            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);
            A.CallTo(() => schemas.FindSchemaByIdAsync(SchemaId, false)).Returns(schema);
        }

        [Fact]
        public async Task Create_should_throw_exception_if_data_is_not_valid()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(invalidData);

            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = invalidData, User = user });

            await TestCreate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Create_should_create_content()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(data);
            A.CallTo(() => schema.ScriptCreate)
                .Returns("<create-script>");

            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = data, User = user });

            await TestCreate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(data, context.Result<EntityCreatedResult<NamedContentData>>().IdOrValue);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<create-script>", "create content")).MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_throw_exception_if_data_is_not_valid()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(invalidData);

            CreateContent();

            var context = CreateContextForCommand(new UpdateContent { ContentId = contentId, Data = invalidData, User = user });

            await TestUpdate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(data);
            A.CallTo(() => schema.ScriptUpdate)
                .Returns("<update-script>");

            CreateContent();

            var context = CreateContextForCommand(new UpdateContent { ContentId = contentId, Data = data, User = user });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(data, context.Result<ContentDataChangedResult>().Data);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>", "update content")).MustHaveHappened();
        }

        [Fact]
        public async Task Patch_should_throw_exception_if_data_is_not_valid()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(invalidData);

            CreateContent();

            var context = CreateContextForCommand(new PatchContent { ContentId = contentId, Data = invalidData, User = user });

            await TestUpdate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Patch_should_update_domain_object()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(data);
            A.CallTo(() => schema.ScriptUpdate)
                .Returns("<update-script>");

            var patch = new NamedContentData().AddField("my-field", new ContentFieldData().SetValue(3));

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(patch);

            CreateContent();

            var context = CreateContextForCommand(new PatchContent { ContentId = contentId, Data = patch, User = user });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.NotNull(context.Result<ContentDataChangedResult>().Data);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>", "patch content")).MustHaveHappened();
        }

        [Fact]
        public async Task Publish_should_publish_domain_object()
        {
            A.CallTo(() => schema.ScriptPublish)
                .Returns("<publish-script>");

            CreateContent();

            var context = CreateContextForCommand(new PublishContent { ContentId = contentId, User = user });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<publish-script>", "publish content")).MustHaveHappened();
        }

        [Fact]
        public async Task Unpublish_should_unpublish_domain_object()
        {
            A.CallTo(() => schema.ScriptUnpublish)
                .Returns("<unpublish-script>");

            CreateContent();

            var context = CreateContextForCommand(new UnpublishContent { ContentId = contentId, User = user });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<unpublish-script>", "unpublish content")).MustHaveHappened();
        }

        [Fact]
        public async Task Delete_should_update_domain_object()
        {
            A.CallTo(() => schema.ScriptDelete)
                .Returns("<delete-script>");

            CreateContent();

            var command = CreateContextForCommand(new DeleteContent { ContentId = contentId, User = user });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(command);
            });

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<delete-script>", "delete content")).MustHaveHappened();
        }

        [Fact]
        public async Task Restore_should_update_domain_object()
        {
            CreateContent();

            content.Delete(new DeleteContent());

            var command = CreateContextForCommand(new RestoreContent { ContentId = contentId, User = user });

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
