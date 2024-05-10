// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Domain.Apps.Entities.Teams.DomainObject.Guards;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;

namespace Squidex.Domain.Teams.Apps.Teams.DomainObject.Guards;

public class GuardTeamTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IUserResolver users = A.Fake<IUserResolver>();
    private readonly IBillingPlans billingPlans = A.Fake<IBillingPlans>();
    private readonly Plan planBasic = new Plan();
    private readonly Plan planFree = new Plan();
    private readonly AuthScheme scheme;

    public GuardTeamTests()
    {
        A.CallTo(() => users.FindByIdOrEmailAsync(A<string>._, default))
            .Returns(A.Dummy<IUser>());

        A.CallTo(() => billingPlans.GetPlan("notfound"))
            .Returns(null!);

        A.CallTo(() => billingPlans.GetPlan("basic"))
            .Returns(planBasic);

        A.CallTo(() => billingPlans.GetPlan("free"))
            .Returns(planFree);

        scheme = new AuthScheme
        {
            Domain = "squidex.io",
            DisplayName = "Squidex",
            Authority = "https://identity.squidex.io",
            ClientId = "clientId",
            ClientSecret = "clientSecret"
        };
    }

    [Fact]
    public void CanCreate_should_throw_exception_if_name_not_valid()
    {
        var command = new CreateTeam { Name = null! };

        ValidationAssert.Throws(() => GuardTeam.CanCreate(command),
            new ValidationError("Name is required.", "Name"));
    }

    [Fact]
    public void CanCreate_should_not_throw_exception_if_team_name_is_valid()
    {
        var command = new CreateTeam { Name = "new-team" };

        GuardTeam.CanCreate(command);
    }

    [Fact]
    public void CanChangePlan_should_throw_exception_if_plan_id_is_null()
    {
        var command = new ChangePlan { Actor = User };

        ValidationAssert.Throws(() => GuardTeam.CanChangePlan(command, billingPlans),
            new ValidationError("Plan ID is required.", "PlanId"));
    }

    [Fact]
    public void CanChangePlan_should_throw_exception_if_plan_not_found()
    {
        var command = new ChangePlan { PlanId = "notfound", Actor = User };

        ValidationAssert.Throws(() => GuardTeam.CanChangePlan(command, billingPlans),
            new ValidationError("A plan with this id does not exist.", "PlanId"));
    }

    [Fact]
    public void CanChangePlan_should_not_throw_exception_if_plan_is_found()
    {
        var command = new ChangePlan { PlanId = "basic", Actor = User };

        GuardTeam.CanChangePlan(command, billingPlans);
    }

    [Fact]
    public async Task CanUpsertAuth_should_not_throw_exception_if_scheme_is_null()
    {
        var command = new UpsertAuth();

        await GuardTeam.CanUpsertAuth(command, AppProvider, CancellationToken);
    }

    [Fact]
    public async Task CanUpsertAuth_should_throw_exception_if_domain_not_defined()
    {
        var command = new UpsertAuth { Scheme = scheme with { Domain = null! } };

        await ValidationAssert.ThrowsAsync(() => GuardTeam.CanUpsertAuth(command, AppProvider, CancellationToken),
            new ValidationError("Domain is required.", "Scheme.Domain"));
    }

    [Fact]
    public async Task CanUpsertAuth_should_throw_exception_if_display_name_not_defined()
    {
        var command = new UpsertAuth { Scheme = scheme with { DisplayName = null! } };

        await ValidationAssert.ThrowsAsync(() => GuardTeam.CanUpsertAuth(command, AppProvider, CancellationToken),
            new ValidationError("Display name is required.", "Scheme.DisplayName"));
    }

    [Fact]
    public async Task CanUpsertAuth_should_throw_exception_if_client_id_not_defined()
    {
        var command = new UpsertAuth { Scheme = scheme with { ClientId = null! } };

        await ValidationAssert.ThrowsAsync(() => GuardTeam.CanUpsertAuth(command, AppProvider, CancellationToken),
            new ValidationError("Client ID is required.", "Scheme.ClientId"));
    }

    [Fact]
    public async Task CanUpsertAuth_should_throw_exception_if_client_secret_not_defined()
    {
        var command = new UpsertAuth { Scheme = scheme with { ClientSecret = null! } };

        await ValidationAssert.ThrowsAsync(() => GuardTeam.CanUpsertAuth(command, AppProvider, CancellationToken),
            new ValidationError("Client Secret is required.", "Scheme.ClientSecret"));
    }

    [Fact]
    public async Task CanUpsertAuth_should_throw_exception_if_authority_not_defined()
    {
        var command = new UpsertAuth { Scheme = scheme with { Authority = null! } };

        await ValidationAssert.ThrowsAsync(() => GuardTeam.CanUpsertAuth(command, AppProvider, CancellationToken),
            new ValidationError("Authority is required.", "Scheme.Authority"));
    }

    [Fact]
    public async Task CanUpsertAuth_should_throw_exception_if_authority_not_valid()
    {
        var command = new UpsertAuth { Scheme = scheme with { Authority = "invalid" } };

        await ValidationAssert.ThrowsAsync(() => GuardTeam.CanUpsertAuth(command, AppProvider, CancellationToken),
            new ValidationError("Authority is not a valid URL.", "Scheme.Authority"));
    }

    [Fact]
    public async Task CanUpsertAuth_should_throw_exception_if_domain_is_already_taken()
    {
        A.CallTo(() => AppProvider.GetTeamByAuthDomainAsync("squidex.io", CancellationToken))
            .Returns(Team);

        var command = new UpsertAuth { Scheme = scheme, TeamId = DomainId.NewGuid() };

        await ValidationAssert.ThrowsAsync(() => GuardTeam.CanUpsertAuth(command, AppProvider, CancellationToken),
            new ValidationError("Domain is already used for another team."));
    }

    [Fact]
    public async Task CanUpsertAuth_should_not_throw_exception_if_command_is_valid()
    {
        A.CallTo(() => AppProvider.GetTeamByAuthDomainAsync("squidex.io", CancellationToken))
            .Returns(Team);

        var command = new UpsertAuth { Scheme = scheme, TeamId = Team.Id };

        await GuardTeam.CanUpsertAuth(command, AppProvider, CancellationToken);
    }

    [Fact]
    public async Task CanDelete_should_throw_exception_if_app_is_assigned()
    {
        A.CallTo(() => AppProvider.GetTeamAppsAsync(TeamId, CancellationToken))
            .Returns([App]);

        var command = new DeleteTeam { TeamId = Team.Id };

        await ValidationAssert.ThrowsAsync(() => GuardTeam.CanDelete(command, AppProvider, CancellationToken),
            new ValidationError("Cannot delete team, when apps are assigned."));
    }

    [Fact]
    public async Task CanDelete_should_not_throw_exception_if_no_app_is_assigned()
    {
        A.CallTo(() => AppProvider.GetTeamAppsAsync(TeamId, CancellationToken))
            .Returns([]);

        var command = new DeleteTeam { TeamId = Team.Id };

        await GuardTeam.CanDelete(command, AppProvider, CancellationToken);
    }
}
