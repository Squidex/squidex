// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public abstract class GivenContext
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private IAppProvider? appProvider;
    private IContextProvider? contextProviderApi;
    private IContextProvider? contextProviderFrontend;
    private Context? contextApi;
    private Context? contextFrontend;

    public DomainId TeamId { get; } = DomainId.NewGuid();

    public NamedId<DomainId> AppId { get; } = NamedId.Of(DomainId.NewGuid(), "my-app");

    public NamedId<DomainId> SchemaId { get; } = NamedId.Of(DomainId.NewGuid(), "my-schema");

    public ITeamEntity Team { get; set; }

    public IAppEntity App { get; set; }

    public ISchemaEntity Schema { get; set; }

    public RefToken User { get; } = RefToken.User("me");

    public RefToken Client { get; } = RefToken.Client("client");

    public Context ApiContext
    {
        get => contextApi ??= new Context(Mocks.ApiUser(), App);
    }

    public Context FrontendContext
    {
        get => contextFrontend ??= new Context(Mocks.FrontendUser(), App);
    }

    public IContextProvider ApiContextProvider
    {
        get => contextProviderApi ??= CreateContextProvider(ApiContext);
    }

    public IContextProvider FrontendContextProvider
    {
        get => contextProviderFrontend ??= CreateContextProvider(FrontendContext);
    }

    public IAppProvider AppProvider
    {
        get => appProvider ??= CreateAppProvider();
    }

    public CancellationToken CancellationToken => cts.Token;

    protected GivenContext()
    {
        App = Mocks.App(AppId, Language.EN, Language.DE);

        Team = Mocks.Team(TeamId, "my-team", User.Identifier);

        Schema = Mocks.Schema(AppId, SchemaId);
    }

    public Context CreateContext(bool isFrontend, params string[] permissions)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim(OpenIdClaims.Subject, User.Identifier));

        if (isFrontend)
        {
            claimsIdentity.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));
        }

        foreach (var permission in permissions)
        {
            claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
        }

        return new Context(claimsPrincipal, App);
    }

    private static IContextProvider CreateContextProvider(Context context)
    {
        var result = A.Fake<IContextProvider>();

        A.CallTo(() => result.Context)
            .Returns(context);

        return result;
    }

    private IAppProvider CreateAppProvider()
    {
        var result = A.Fake<IAppProvider>();

        A.CallTo(() => result.GetAppAsync(AppId.Id, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(() => App);

        A.CallTo(() => result.GetTeamAsync(TeamId, A<CancellationToken>._))
            .ReturnsLazily(() => Team);

        A.CallTo(() => result.GetSchemaAsync(AppId.Id, SchemaId.Id, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(() => Schema);

        A.CallTo(() => result.GetSchemaAsync(AppId.Id, SchemaId.Name, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(() => Schema);

        A.CallTo(() => result.GetAppWithSchemaAsync(AppId.Id, SchemaId.Id, A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(() => (App, Schema));

        return result;
    }
}
