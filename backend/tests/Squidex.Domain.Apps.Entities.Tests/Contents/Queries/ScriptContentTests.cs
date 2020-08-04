﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ScriptContentTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly NamedId<Guid> schemaWithScriptId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly ProvideSchema schemaProvider;
        private readonly ScriptContent sut;

        public ScriptContentTests()
        {
            var schemaDef = new Schema(schemaId.Name);

            var schemaDefWithScript =
                new Schema(schemaWithScriptId.Name)
                    .SetScripts(new SchemaScripts
                    {
                        Query = "my-query"
                    });

            schemaProvider = x =>
            {
                if (x == schemaId.Id)
                {
                    return Task.FromResult(Mocks.Schema(appId, schemaId, schemaDef));
                }
                else if (x == schemaWithScriptId.Id)
                {
                    return Task.FromResult(Mocks.Schema(appId, schemaWithScriptId, schemaDefWithScript));
                }
                else
                {
                    throw new DomainObjectNotFoundException(x.ToString());
                }
            };

            sut = new ScriptContent(scriptEngine);
        }

        [Fact]
        public async Task Should_not_call_script_engine_when_no_script_configured()
        {
            var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

            var content = new ContentEntity { SchemaId = schemaId };

            await sut.EnrichAsync(ctx, new[] { content }, schemaProvider);

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, A<string>._, ScriptOptions()))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_call_script_engine_for_frontend_user()
        {
            var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            var content = new ContentEntity { SchemaId = schemaWithScriptId };

            await sut.EnrichAsync(ctx, new[] { content }, schemaProvider);

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, A<string>._, ScriptOptions()))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_call_script_engine_with_data()
        {
            var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

            var oldData = new NamedContentData();

            var content = new ContentEntity { SchemaId = schemaWithScriptId, Data = oldData };

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, "my-query", ScriptOptions()))
                .Returns(new NamedContentData());

            await sut.EnrichAsync(ctx, new[] { content }, schemaProvider);

            Assert.NotSame(oldData, content.Data);

            A.CallTo(() => scriptEngine.TransformAsync(
                    A<ScriptVars>.That.Matches(x =>
                        ReferenceEquals(x.User, ctx.User) &&
                        ReferenceEquals(x.Data, oldData) &&
                        x.ContentId == content.Id),
                    "my-query",
                    ScriptOptions()))
                .MustHaveHappened();
        }

        private ScriptOptions ScriptOptions()
        {
            return A<ScriptOptions>.That.Matches(x => x.AsContext);
        }
    }
}
