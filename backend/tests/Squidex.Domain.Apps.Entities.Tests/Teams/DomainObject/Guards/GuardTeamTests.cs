// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Domain.Apps.Entities.Teams.DomainObject.Guards;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;

namespace Squidex.Domain.Teams.Apps.Teams.DomainObject.Guards;

public class GuardTeamTests : IClassFixture<TranslationsFixture>
{
    private readonly IUserResolver users = A.Fake<IUserResolver>();
    private readonly IBillingPlans billingPlans = A.Fake<IBillingPlans>();
    private readonly Plan planBasic = new Plan();
    private readonly Plan planFree = new Plan();

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
        var command = new ChangePlan { Actor = RefToken.User("me") };

        ValidationAssert.Throws(() => GuardTeam.CanChangePlan(command, billingPlans),
            new ValidationError("Plan ID is required.", "PlanId"));
    }

    [Fact]
    public void CanChangePlan_should_throw_exception_if_plan_not_found()
    {
        var command = new ChangePlan { PlanId = "notfound", Actor = RefToken.User("me") };

        ValidationAssert.Throws(() => GuardTeam.CanChangePlan(command, billingPlans),
            new ValidationError("A plan with this id does not exist.", "PlanId"));
    }

    [Fact]
    public void CanChangePlan_should_not_throw_exception_if_plan_is_found()
    {
        var command = new ChangePlan { PlanId = "basic", Actor = RefToken.User("me") };

        GuardTeam.CanChangePlan(command, billingPlans);
    }
}
