﻿// ==========================================================================
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
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentCommandMiddlewareTests : HandlerTestBase<ContentDomainObject>
    {
        private readonly ContentCommandMiddleware sut;
        private readonly ContentDomainObject content;
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly ClaimsPrincipal user = new ClaimsPrincipal();
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.DE);
        private readonly Guid contentId = Guid.NewGuid();

        private readonly NamedContentData invalidData =
            new NamedContentData()
                .AddField("my-field1", new ContentFieldData()
                    .AddValue(null))
                .AddField("my-field2", new ContentFieldData()
                    .AddValue(1));
        private readonly NamedContentData data =
            new NamedContentData()
                .AddField("my-field1", new ContentFieldData()
                    .AddValue(1))
                .AddField("my-field2", new ContentFieldData()
                    .AddValue(1));
        private readonly NamedContentData patch =
            new NamedContentData()
                .AddField("my-field1", new ContentFieldData()
                    .AddValue(1));

        public ContentCommandMiddlewareTests()
        {
            var schemaDef =
                new Schema("my-schema")
                    .AddField(new NumberField(1, "my-field1", Partitioning.Invariant,
                        new NumberFieldProperties { IsRequired = true }))
                    .AddField(new NumberField(2, "my-field2", Partitioning.Invariant,
                        new NumberFieldProperties { IsRequired = false }));

            content = new ContentDomainObject(contentId, -1);

            sut = new ContentCommandMiddleware(Handler, appProvider, A.Dummy<IAssetRepository>(), scriptEngine, A.Dummy<IContentRepository>());

            A.CallTo(() => app.LanguagesConfig).Returns(languagesConfig);

            A.CallTo(() => appProvider.GetAppAsync(AppName)).Returns(app);

            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);
            A.CallTo(() => schema.ScriptCreate).Returns("<create-script>");
            A.CallTo(() => schema.ScriptChange).Returns("<change-script>");
            A.CallTo(() => schema.ScriptUpdate).Returns("<update-script>");
            A.CallTo(() => schema.ScriptDelete).Returns("<delete-script>");

            A.CallTo(() => appProvider.GetAppWithSchemaAsync(AppName, SchemaId)).Returns((app, schema));
        }

        [Fact]
        public async Task Create_should_throw_exception_if_data_is_not_valid()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored))
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
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .Returns(data);

            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = data, User = user });

            await TestCreate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(data, context.Result<EntityCreatedResult<NamedContentData>>().IdOrValue);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<create-script>")).MustHaveHappened();
            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>")).MustNotHaveHappened();
        }

        [Fact]
        public async Task Create_should_also_invoke_publish_script_when_publishing()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .Returns(data);

            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = data, User = user, Publish = true });

            await TestCreate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(data, context.Result<EntityCreatedResult<NamedContentData>>().IdOrValue);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<create-script>")).MustHaveHappened();
            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>")).MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_throw_exception_if_data_is_not_valid()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .Returns(invalidData);

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
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .Returns(data);

            CreateContent();

            var context = CreateContextForCommand(new UpdateContent { ContentId = contentId, Data = data, User = user });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(data, context.Result<ContentDataChangedResult>().Data);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>")).MustHaveHappened();
        }

        [Fact]
        public async Task Patch_should_throw_exception_if_data_is_not_valid()
        {
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored))
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
            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .Returns(data);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored)).Returns(patch);

            CreateContent();

            var context = CreateContextForCommand(new PatchContent { ContentId = contentId, Data = patch, User = user });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.NotNull(context.Result<ContentDataChangedResult>().Data);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>")).MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_publish_domain_object()
        {
            CreateContent();

            var context = CreateContextForCommand(new ChangeContentStatus { ContentId = contentId, User = user, Status = Status.Published });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>")).MustHaveHappened();
        }

        [Fact]
        public async Task Delete_should_update_domain_object()
        {
            CreateContent();

            var command = CreateContextForCommand(new DeleteContent { ContentId = contentId, User = user });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(command);
            });

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<delete-script>")).MustHaveHappened();
        }

        private void CreateContent()
        {
            content.Create(new CreateContent { Data = data });
        }
    }
}
