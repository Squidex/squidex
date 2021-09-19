// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards
{
    public class GuardAppTests : IClassFixture<TranslationsFixture>
    {
        private readonly IUserResolver users = A.Fake<IUserResolver>();
        private readonly IAppPlansProvider appPlans = A.Fake<IAppPlansProvider>();
        private readonly IAppLimitsPlan basicPlan = A.Fake<IAppLimitsPlan>();
        private readonly IAppLimitsPlan freePlan = A.Fake<IAppLimitsPlan>();

        public GuardAppTests()
        {
            A.CallTo(() => users.FindByIdOrEmailAsync(A<string>._, default))
                .Returns(A.Dummy<IUser>());

            A.CallTo(() => appPlans.GetPlan("notfound"))
                .Returns(null!);

            A.CallTo(() => appPlans.GetPlan("basic"))
                .Returns(basicPlan);

            A.CallTo(() => appPlans.GetPlan("free"))
                .Returns(freePlan);
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
            var command = new ChangePlan { Actor = RefToken.User("me") };

            AppPlan? plan = null;

            ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, App(plan), appPlans),
                new ValidationError("Plan ID is required.", "PlanId"));
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_not_found()
        {
            var command = new ChangePlan { PlanId = "notfound", Actor = RefToken.User("me") };

            AppPlan? plan = null;

            ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, App(plan), appPlans),
                new ValidationError("A plan with this id does not exist.", "PlanId"));
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_was_configured_from_another_user()
        {
            var command = new ChangePlan { PlanId = "basic", Actor = RefToken.User("me") };

            var plan = new AppPlan(RefToken.User("other"), "premium");

            ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, App(plan), appPlans),
                new ValidationError("Plan can only changed from the user who configured the plan initially."));
        }

        [Fact]
        public void CanChangePlan_should_not_throw_exception_if_plan_is_the_same()
        {
            var command = new ChangePlan { PlanId = "basic", Actor = RefToken.User("me") };

            var plan = new AppPlan(command.Actor, "basic");

            GuardApp.CanChangePlan(command, App(plan), appPlans);
        }

        [Fact]
        public void CanChangePlan_should_not_throw_exception_if_same_user_but_other_plan()
        {
            var command = new ChangePlan { PlanId = "basic", Actor = RefToken.User("me") };

            var plan = new AppPlan(command.Actor, "premium");

            GuardApp.CanChangePlan(command, App(plan), appPlans);
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
                    Patterns = ImmutableList.Create(
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
                    Patterns = ImmutableList.Create(
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
                    Editors = ImmutableList.Create(
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
                    Editors = ImmutableList.Create(
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
                    Patterns = ImmutableList.Create(
                        new Pattern("name", "[a-z]")),
                    Editors = ImmutableList.Create(
                        new Editor("name", "url/to/editor"))
                }
            };

            GuardApp.CanUpdateSettings(command);
        }

        private static IAppEntity App(AppPlan? plan)
        {
            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.Plan)
                .Returns(plan);

            return app;
        }
    }
}
