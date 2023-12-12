// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ScriptContentTests : GivenContext
{
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly ScriptContent sut;

    public ScriptContentTests()
    {
        sut = new ScriptContent(scriptEngine);
    }

    [Fact]
    public async Task Should_not_call_script_engine_if_no_script_configured()
    {
        var content = CreateContent();

        await sut.EnrichAsync(ApiContext, new[] { content }, SchemaProvider(), CancellationToken);

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_script_engine_for_frontend_user()
    {
        Schema = Schema.SetScripts(new SchemaScripts
        {
            Query = "my-query"
        });

        var content = CreateContent();

        await sut.EnrichAsync(FrontendContext, new[] { content }, SchemaProvider(), CancellationToken);

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_script_engine_if_disabled_and_user_has_permission()
    {
        Schema = Schema.SetScripts(new SchemaScripts
        {
            Query = "my-query"
        });

        var content = CreateContent();

        await sut.EnrichAsync(ContextWithNoScript(), new[] { content }, SchemaProvider(), CancellationToken);

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_call_script_engine()
    {
        Schema = Schema.SetScripts(new SchemaScripts
        {
            Query = "my-query"
        });

        var contentBefore = CreateContent();
        var contentData = contentBefore.Data;

        await sut.EnrichAsync(ApiContext, new[] { contentBefore }, SchemaProvider(), CancellationToken);

        Assert.NotSame(contentBefore.Data, contentData);

        A.CallTo(() => scriptEngine.TransformAsync(
                A<DataScriptVars>.That.Matches(x =>
                    Equals(x["contentId"], contentBefore.Id) &&
                    Equals(x["data"], contentData) &&
                    Equals(x["appId"], AppId.Id) &&
                    Equals(x["appName"], AppId.Name) &&
                    Equals(x["user"], ApiContext.UserPrincipal)),
                "my-query",
                ScriptOptions(),
                CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_call_script_engine_with_pre_query_script()
    {
        Schema = Schema.SetScripts(new SchemaScripts
        {
            Query = "my-query",
            QueryPre = "my-pre-query",
        });

        var contentBefore = CreateContent();
        var contentData = contentBefore.Data;

        await sut.EnrichAsync(ApiContext, new[] { contentBefore }, SchemaProvider(), CancellationToken);

        Assert.NotSame(contentBefore.Data, contentData);

        A.CallTo(() => scriptEngine.ExecuteAsync(
                A<DataScriptVars>.That.Matches(x =>
                    Equals(x.GetValue<object>("contentId"), null) &&
                    Equals(x["appId"], AppId.Id) &&
                    Equals(x["appName"], AppId.Name) &&
                    Equals(x["user"], ApiContext.UserPrincipal)),
                "my-pre-query",
                ScriptOptions(),
                CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => scriptEngine.TransformAsync(
                A<DataScriptVars>.That.Matches(x =>
                    Equals(x["contentId"], contentBefore.Id) &&
                    Equals(x["data"], contentData) &&
                    Equals(x["appId"], AppId.Id) &&
                    Equals(x["appName"], AppId.Name) &&
                    Equals(x["user"], ApiContext.UserPrincipal)),
                "my-query",
                ScriptOptions(),
                CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_make_test_with_pre_query_script()
    {
        Schema = Schema.SetScripts(new SchemaScripts
        {
            Query = @"
                    ctx.data.test = { iv: ctx.custom };
                    replace()",
            QueryPre = "ctx.custom = 123;"
        });

        var content = CreateContent();

        var realScriptEngine =
            new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
                Options.Create(new JintScriptOptions
                {
                    TimeoutScript = TimeSpan.FromSeconds(20),
                    TimeoutExecution = TimeSpan.FromSeconds(100)
                }));

        var sut2 = new ScriptContent(realScriptEngine);

        await sut2.EnrichAsync(ApiContext, new[] { content }, SchemaProvider(), CancellationToken);

        Assert.Equal(JsonValue.Create(123), content.Data["test"]!["iv"]);
    }

    private ProvideSchema SchemaProvider()
    {
        return x => Task.FromResult((Schema, ResolvedComponents.Empty));
    }

    private static ScriptOptions ScriptOptions()
    {
        return A<ScriptOptions>.That.Matches(x => x.AsContext);
    }

    private Context ContextWithNoScript()
    {
        var contextPermission = PermissionIds.ForApp(PermissionIds.AppNoScripting, App.Name).Id;
        var contextInstance = CreateContext(false, contextPermission).Clone(b => b.WithNoScripting());

        return contextInstance;
    }
}
