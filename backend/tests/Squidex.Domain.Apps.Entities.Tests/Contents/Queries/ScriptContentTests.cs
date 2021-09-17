// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
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
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly NamedId<DomainId> schemaWithScriptId = NamedId.Of(DomainId.NewGuid(), "my-schema");
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
                    return Task.FromResult((Mocks.Schema(appId, schemaId, schemaDef), ResolvedComponents.Empty));
                }
                else if (x == schemaWithScriptId.Id)
                {
                    return Task.FromResult((Mocks.Schema(appId, schemaWithScriptId, schemaDefWithScript), ResolvedComponents.Empty));
                }
                else
                {
                    throw new DomainObjectNotFoundException(x.ToString());
                }
            };

            sut = new ScriptContent(scriptEngine);
        }

        [Fact]
        public async Task Should_not_call_script_engine_if_no_script_configured()
        {
            var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

            var content = new ContentEntity { SchemaId = schemaId };

            await sut.EnrichAsync(ctx, new[] { content }, schemaProvider, default);

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_call_script_engine_for_frontend_user()
        {
            var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            var content = new ContentEntity { SchemaId = schemaWithScriptId };

            await sut.EnrichAsync(ctx, new[] { content }, schemaProvider, default);

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_call_script_engine_with_data()
        {
            var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

            var oldData = new ContentData();

            var content = new ContentEntity { SchemaId = schemaWithScriptId, Data = oldData };

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, "my-query", ScriptOptions(), A<CancellationToken>._))
                .Returns(new ContentData());

            await sut.EnrichAsync(ctx, new[] { content }, schemaProvider, default);

            Assert.NotSame(oldData, content.Data);

            A.CallTo(() => scriptEngine.TransformAsync(
                    A<ScriptVars>.That.Matches(x =>
                        Equals(x["user"], ctx.User) &&
                        Equals(x["data"], oldData) &&
                        Equals(x["contentId"], content.Id)),
                    "my-query",
                    ScriptOptions(), A<CancellationToken>._))
                .MustHaveHappened();
        }

        private static ScriptOptions ScriptOptions()
        {
            return A<ScriptOptions>.That.Matches(x => x.AsContext);
        }
    }
}
