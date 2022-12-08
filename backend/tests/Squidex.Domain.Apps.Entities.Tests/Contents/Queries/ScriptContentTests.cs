// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ScriptContentTests
{
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly ScriptContent sut;

    public ScriptContentTests()
    {
        sut = new ScriptContent(scriptEngine);
    }

    [Fact]
    public async Task Should_not_call_script_engine_if_no_script_configured()
    {
        var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

        var (provider, schemaId) = CreateSchema(
            queryPre: "my-pre-query");

        var content = new ContentEntity { Data = new ContentData(), SchemaId = schemaId };

        await sut.EnrichAsync(ctx, new[] { content }, provider, default);

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_script_engine_for_frontend_user()
    {
        var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

        var (provider, schemaId) = CreateSchema(
            query: "my-query");

        var content = new ContentEntity { Data = new ContentData(), SchemaId = schemaId };

        await sut.EnrichAsync(ctx, new[] { content }, provider, default);

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_call_script_engine_with_data()
    {
        var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

        var oldData = new ContentData();

        var (provider, schemaId) = CreateSchema(
            query: "my-query");

        var content = new ContentEntity { Data = oldData, SchemaId = schemaId };

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, "my-query", ScriptOptions(), A<CancellationToken>._))
            .Returns(new ContentData());

        await sut.EnrichAsync(ctx, new[] { content }, provider, default);

        Assert.NotSame(oldData, content.Data);

        A.CallTo(() => scriptEngine.TransformAsync(
                A<DataScriptVars>.That.Matches(x =>
                    Equals(x["user"], ctx.UserPrincipal) &&
                    Equals(x["data"], oldData) &&
                    Equals(x["contentId"], content.Id)),
                "my-query",
                ScriptOptions(), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_make_test_with_pre_query_script()
    {
        var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

        var (provider, id) = CreateSchema(
            query: @"
                    ctx.data.test = { iv: ctx.custom };
                    replace()",
            queryPre: "ctx.custom = 123;");

        var content = new ContentEntity { Data = new ContentData(), SchemaId = id };

        var realScriptEngine =
            new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
                Options.Create(new JintScriptOptions
                {
                    TimeoutScript = TimeSpan.FromSeconds(20),
                    TimeoutExecution = TimeSpan.FromSeconds(100)
                }));

        var sut2 = new ScriptContent(realScriptEngine);

        await sut2.EnrichAsync(ctx, new[] { content }, provider, default);

        Assert.Equal(JsonValue.Create(123), content.Data["test"]!["iv"]);
    }

    private (ProvideSchema, NamedId<DomainId>) CreateSchema(string? query = null, string? queryPre = null)
    {
        var id = NamedId.Of(DomainId.NewGuid(), "my-schema");

        return (_ =>
        {
            var schemaDef =
                new Schema(id.Name)
                    .SetScripts(new SchemaScripts
                    {
                        Query = query,
                        QueryPre = queryPre
                    });

            return Task.FromResult((Mocks.Schema(appId, id, schemaDef), ResolvedComponents.Empty));
        }, id);
    }

    private static ScriptOptions ScriptOptions()
    {
        return A<ScriptOptions>.That.Matches(x => x.AsContext);
    }
}
