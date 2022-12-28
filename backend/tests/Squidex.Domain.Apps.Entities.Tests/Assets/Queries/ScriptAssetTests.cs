// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Assets.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class ScriptAssetTests
{
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly ScriptAsset sut;

    public ScriptAssetTests()
    {
        sut = new ScriptAsset(scriptEngine);
    }

    [Fact]
    public async Task Should_not_call_script_engine_if_no_script_configured()
    {
        var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

        var asset = new AssetEntity();

        await sut.EnrichAsync(ctx, new[] { asset }, default);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<AssetScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_script_engine_for_frontend_user()
    {
        var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

        A.CallTo(() => ctx.App.AssetScripts)
            .Returns(new AssetScripts { Query = "my-query" });

        var asset = new AssetEntity();

        await sut.EnrichAsync(ctx, new[] { asset }, default);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<AssetScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_call_script_engine()
    {
        var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

        A.CallTo(() => ctx.App.AssetScripts)
            .Returns(new AssetScripts { Query = "my-query" });

        var asset = new AssetEntity { Id = DomainId.NewGuid() };

        await sut.EnrichAsync(ctx, new[] { asset }, default);

        A.CallTo(() => scriptEngine.ExecuteAsync(
                A<AssetScriptVars>.That.Matches(x =>
                    Equals(x["assetId"], asset.Id) &&
                    Equals(x["appId"], appId.Id) &&
                    Equals(x["appName"], appId.Name) &&
                    Equals(x["user"], ctx.UserPrincipal)),
                "my-query",
                ScriptOptions(), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_make_test_with_pre_query_script()
    {
        var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

        A.CallTo(() => ctx.App.AssetScripts)
            .Returns(new AssetScripts { Query = "my-query", QueryPre = "my-pre-query" });

        var asset = new AssetEntity { Id = DomainId.NewGuid() };

        await sut.EnrichAsync(ctx, new[] { asset }, default);

        A.CallTo(() => scriptEngine.ExecuteAsync(
                A<AssetScriptVars>.That.Matches(x =>
                    Equals(x.GetValue<object>("assetId"), null) &&
                    Equals(x["appId"], appId.Id) &&
                    Equals(x["appName"], appId.Name) &&
                    Equals(x["user"], ctx.UserPrincipal)),
                "my-pre-query",
                ScriptOptions(), A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => scriptEngine.ExecuteAsync(
                A<AssetScriptVars>.That.Matches(x =>
                    Equals(x["assetId"], asset.Id) &&
                    Equals(x["appId"], appId.Id) &&
                    Equals(x["appName"], appId.Name) &&
                    Equals(x["user"], ctx.UserPrincipal)),
                "my-query",
                ScriptOptions(), A<CancellationToken>._))
            .MustHaveHappened();
    }

    private static ScriptOptions ScriptOptions()
    {
        return A<ScriptOptions>.That.Matches(x => x.AsContext);
    }
}
