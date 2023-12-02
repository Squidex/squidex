// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards;

public class GuardAppTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IUserResolver users = A.Fake<IUserResolver>();
    private readonly IBillingPlans billingPlans = A.Fake<IBillingPlans>();

    public GuardAppTests()
    {
        A.CallTo(() => users.FindByIdOrEmailAsync(A<string>._, default))
            .Returns(A.Dummy<IUser>());

        A.CallTo(() => billingPlans.GetPlan("notfound"))
            .Returns(null!);

        A.CallTo(() => billingPlans.GetPlan("basic"))
            .Returns(new Plan());

        App = App with
        {
            TeamId = default
        };

        Team = Team with
        {
            Contributors = Contributors.Empty.Assign(User.Identifier, Role.Owner)
        };
    }

    [Fact]
    public void CanCreate_should_throw_exception_if_name_not_valid()
    {
        var command = new CreateApp { Name = "INVALID NAME" };

        ValidationAssert.Throws(() => GuardApp.CanCreate(command),
            new ValidationError("Name is not a valid slug.", "Name"));
    }

    [Fact]
    public void CanCreate_should_not_throw_exception_if_app_name_is_valid()
    {
        var command = new CreateApp { Name = "new-app" };

        GuardApp.CanCreate(command);
    }

    [Fact]
    public void CanUploadImage_should_throw_exception_if_name_not_valid()
    {
        var command = new UploadAppImage();

        ValidationAssert.Throws(() => GuardApp.CanUploadImage(command),
            new ValidationError("File is required.", "File"));
    }

    [Fact]
    public void CanUploadImage_should_not_throw_exception_if_app_name_is_valid()
    {
        var command = new UploadAppImage { File = new NoopAssetFile() };

        GuardApp.CanUploadImage(command);
    }

    [Fact]
    public void CanChangePlan_should_throw_exception_if_plan_id_is_null()
    {
        var command = new ChangePlan { Actor = User };

        ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, App, billingPlans),
            new ValidationError("Plan ID is required.", "PlanId"));
    }

    [Fact]
    public void CanChangePlan_should_throw_exception_if_plan_not_found()
    {
        var command = new ChangePlan { PlanId = "notfound", Actor = User };

        ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, App, billingPlans),
            new ValidationError("A plan with this id does not exist.", "PlanId"));
    }

    [Fact]
    public void CanChangePlan_should_throw_exception_if_plan_was_configured_from_another_user()
    {
        var command = new ChangePlan { PlanId = "basic", Actor = User };

        App = App with
        {
            Plan = new AssignedPlan(RefToken.User("other"), "premium")
        };

        ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, App, billingPlans),
            new ValidationError("Plan can only changed from the user who configured the plan initially."));
    }

    [Fact]
    public void CanChangePlan_should_throw_exception_if_assigned_to_team()
    {
        var command = new ChangePlan { PlanId = "basic", Actor = User };

        App = App with
        {
            TeamId = Team.Id
        };

        ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, App, billingPlans),
            new ValidationError("Plan is managed by the team."));
    }

    [Fact]
    public void CanChangePlan_should_not_throw_exception_if_plan_is_the_same()
    {
        var command = new ChangePlan { PlanId = "basic", Actor = User };

        App = App with
        {
            Plan = new AssignedPlan(User, "premium")
        };

        GuardApp.CanChangePlan(command, App, billingPlans);
    }

    [Fact]
    public void CanChangePlan_should_not_throw_exception_if_same_user_but_other_plan()
    {
        var command = new ChangePlan { PlanId = "basic", Actor = User };

        App = App with
        {
            Plan = new AssignedPlan(User, "premium")
        };

        GuardApp.CanChangePlan(command, App, billingPlans);
    }

    [Fact]
    public async Task CanTransfer_should_not_throw_exception_if_team_exists()
    {
        var command = new TransferToTeam { TeamId = TeamId, Actor = User };

        await GuardApp.CanTransfer(command, App, AppProvider, default);
    }

    [Fact]
    public async Task CanTransfer_should_not_throw_exception_if_user_has_transfer_permission()
    {
        var admin = Mocks.ApiUser(permission: PermissionIds.Transfer);

        var command = new TransferToTeam { TeamId = TeamId, Actor = User, User = admin };

        await GuardApp.CanTransfer(command, App, AppProvider, default);
    }

    [Fact]
    public async Task CanTransfer_should_throw_exception_if_team_does_not_exist()
    {
        Team = null!;

        var command = new TransferToTeam { TeamId = TeamId, Actor = User };

        await ValidationAssert.ThrowsAsync(() => GuardApp.CanTransfer(command, App, AppProvider, default),
            new ValidationError("The team does not exist."));
    }

    [Fact]
    public async Task CanTransfer_should_throw_exception_if_actor_is_not_part_of_team()
    {
        var nonContributor = RefToken.User("Other");

        var command = new TransferToTeam { TeamId = TeamId, Actor = nonContributor };

        await ValidationAssert.ThrowsAsync(() => GuardApp.CanTransfer(command, App, AppProvider, default),
            new ValidationError("The team does not exist."));
    }

    [Fact]
    public async Task CanTransfer_should_throw_exception_if_app_has_plan()
    {
        var command = new TransferToTeam { TeamId = TeamId, Actor = User };

        App = App with
        {
            Plan = new AssignedPlan(RefToken.User("other"), "premium")
        };

        await ValidationAssert.ThrowsAsync(() => GuardApp.CanTransfer(command, App, AppProvider, default),
            new ValidationError("Subscription must be cancelled first before the app can be transfered."));
    }

    [Fact]
    public void CanUpdateSettings_should_throw_exception_if_settings_is_null()
    {
        var command = new UpdateAppSettings();

        ValidationAssert.Throws(() => GuardApp.CanUpdateSettings(command),
            new ValidationError("Settings is required.", "Settings"));
    }

    [Fact]
    public void CanUpdateSettings_should_throw_exception_if_patterns_is_null()
    {
        var command = new UpdateAppSettings
        {
            Settings = new AppSettings
            {
                Patterns = null!
            }
        };

        ValidationAssert.Throws(() => GuardApp.CanUpdateSettings(command),
            new ValidationError("Patterns is required.", "Settings.Patterns"));
    }

    [Fact]
    public void CanUpdateSettings_should_throw_exception_if_patterns_has_null_name()
    {
        var command = new UpdateAppSettings
        {
            Settings = new AppSettings
            {
                Patterns = ReadonlyList.Create(
                    new Pattern(null!, "[a-z]"))
            }
        };

        ValidationAssert.Throws(() => GuardApp.CanUpdateSettings(command),
            new ValidationError("Name is required.", "Settings.Patterns[0].Name"));
    }

    [Fact]
    public void CanUpdateSettings_should_throw_exception_if_patterns_has_null_regex()
    {
        var command = new UpdateAppSettings
        {
            Settings = new AppSettings
            {
                Patterns = ReadonlyList.Create(
                    new Pattern("name", null!))
            }
        };

        ValidationAssert.Throws(() => GuardApp.CanUpdateSettings(command),
            new ValidationError("Regex is required.", "Settings.Patterns[0].Regex"));
    }

    [Fact]
    public void CanUpdateSettings_should_throw_exception_if_editors_is_null()
    {
        var command = new UpdateAppSettings
        {
            Settings = new AppSettings
            {
                Editors = null!
            }
        };

        ValidationAssert.Throws(() => GuardApp.CanUpdateSettings(command),
            new ValidationError("Editors is required.", "Settings.Editors"));
    }

    [Fact]
    public void CanUpdateSettings_should_throw_exception_if_editors_has_null_name()
    {
        var command = new UpdateAppSettings
        {
            Settings = new AppSettings
            {
                Editors = ReadonlyList.Create(
                    new Editor(null!, "[a-z]"))
            }
        };

        ValidationAssert.Throws(() => GuardApp.CanUpdateSettings(command),
            new ValidationError("Name is required.", "Settings.Editors[0].Name"));
    }

    [Fact]
    public void CanUpdateSettings_should_throw_exception_if_patterns_has_null_url()
    {
        var command = new UpdateAppSettings
        {
            Settings = new AppSettings
            {
                Editors = ReadonlyList.Create(
                    new Editor("name", null!))
            }
        };

        ValidationAssert.Throws(() => GuardApp.CanUpdateSettings(command),
            new ValidationError("Url is required.", "Settings.Editors[0].Url"));
    }

    [Fact]
    public void CanUpdateSettings_should_not_throw_exception_if_setting_is_valid()
    {
        var command = new UpdateAppSettings
        {
            Settings = new AppSettings
            {
                Patterns = ReadonlyList.Create(
                    new Pattern("name", "[a-z]")),
                Editors = ReadonlyList.Create(
                    new Editor("name", "url/to/editor"))
            }
        };

        GuardApp.CanUpdateSettings(command);
    }
}
