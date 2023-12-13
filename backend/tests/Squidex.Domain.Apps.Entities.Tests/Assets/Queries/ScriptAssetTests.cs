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
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class ScriptAssetTests : GivenContext
{
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly ScriptAsset sut;

    public ScriptAssetTests()
    {
        sut = new ScriptAsset(scriptEngine);
    }

    [Fact]
    public async Task Should_not_call_script_engine_if_no_script_configured()
    {
        var asset = CreateAsset();

        await sut.EnrichAsync(ApiContext, new[] { asset }, CancellationToken);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<AssetScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_script_engine_for_frontend_user()
    {
        SetupScript(query: "my-query");

        var asset = CreateAsset();

        await sut.EnrichAsync(FrontendContext, new[] { asset }, CancellationToken);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<AssetScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_script_engine_if_disabled_and_user_has_permission()
    {
        SetupScript(query: "my-query");

        var asset = CreateAsset();

        await sut.EnrichAsync(ContextWithNoScript(), new[] { asset }, CancellationToken);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<AssetScriptVars>._, A<string>._, ScriptOptions(), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_call_script_engine()
    {
        SetupScript(query: "my-query");

        var asset = CreateAsset();

        await sut.EnrichAsync(ApiContext, new[] { asset }, CancellationToken);

        A.CallTo(() => scriptEngine.ExecuteAsync(
                A<AssetScriptVars>.That.Matches(x =>
                    Equals(x["assetId"], asset.Id) &&
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
        SetupScript(query: "my-query", queryPre: "my-pre-query");

        var asset = CreateAsset();

        await sut.EnrichAsync(ApiContext, new[] { asset }, CancellationToken);

        A.CallTo(() => scriptEngine.ExecuteAsync(
                A<AssetScriptVars>.That.Matches(x =>
                    Equals(x.GetValue<object>("assetId"), null) &&
                    Equals(x["appId"], AppId.Id) &&
                    Equals(x["appName"], AppId.Name) &&
                    Equals(x["user"], ApiContext.UserPrincipal)),
                "my-pre-query",
                ScriptOptions(),
                CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => scriptEngine.ExecuteAsync(
                A<AssetScriptVars>.That.Matches(x =>
                    Equals(x.GetValue<object>("assetId"), asset.Id) &&
                    Equals(x["appId"], AppId.Id) &&
                    Equals(x["appName"], AppId.Name) &&
                    Equals(x["user"], ApiContext.UserPrincipal)),
                "my-query",
                ScriptOptions(),
                CancellationToken))
            .MustHaveHappened();
    }

    private void SetupScript(string? query = null, string? queryPre = null)
    {
        App = App with
        {
            AssetScripts = new AssetScripts
            {
                Query = query,
                QueryPre = queryPre
            }
        };
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
