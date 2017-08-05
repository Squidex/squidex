// ==========================================================================
//  AppCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Apps.Services.Implementations;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Shared.Users;
using Xunit;

// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppCommandHandlerTests : HandlerTestBase<AppDomainObject>
    {
        private readonly IAppRepository appRepository = A.Fake<IAppRepository>();
        private readonly IAppPlansProvider appPlansProvider = A.Fake<IAppPlansProvider>();
        private readonly IAppPlanBillingManager appPlansBillingManager = A.Fake<IAppPlanBillingManager>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly AppCommandHandler sut;
        private readonly AppDomainObject app;
        private readonly Language language = Language.DE;
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientName = "client";

        public AppCommandHandlerTests()
        {
            app = new AppDomainObject(AppId, -1);

            sut = new AppCommandHandler(Handler, appRepository, appPlansProvider, appPlansBillingManager, userResolver);
        }

        [Fact]
        public async Task Create_should_throw_exception_if_a_name_with_same_name_already_exists()
        {
            var context = CreateContextForCommand(new CreateApp { Name = AppName, AppId = AppId });

            A.CallTo(() => appRepository.FindAppAsync(AppName))
                .Returns(Task.FromResult(A.Dummy<IAppEntity>()));

            await TestCreate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(async () => await sut.HandleAsync(context));
            }, false);

            A.CallTo(() => appRepository.FindAppAsync(AppName)).MustHaveHappened();
        }

        [Fact]
        public async Task Create_should_create_app_if_name_is_free()
        {
            var context = CreateContextForCommand(new CreateApp { Name = AppName, AppId = AppId });

            A.CallTo(() => appRepository.FindAppAsync(AppName))
                .Returns(Task.FromResult<IAppEntity>(null));

            await TestCreate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(AppId, context.Result<EntityCreatedResult<Guid>>().IdOrValue);
        }

        [Fact]
        public async Task AssignContributor_should_throw_exception_if_user_not_found()
        {
            CreateApp();

            var context = CreateContextForCommand(new AssignContributor { ContributorId = contributorId });

            A.CallTo(() => userResolver.FindByIdAsync(contributorId))
                .Returns(Task.FromResult<IUser>(null));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_throw_exception_if_reached_max_contributor_size()
        {
            A.CallTo(() => appPlansProvider.GetPlan(null))
                .Returns(new ConfigAppLimitsPlan { MaxContributors = 2 });

            CreateApp()
                .AssignContributor(CreateCommand(new AssignContributor { ContributorId = "1" }))
                .AssignContributor(CreateCommand(new AssignContributor { ContributorId = "2" }));

            var context = CreateContextForCommand(new AssignContributor { ContributorId = contributorId });

            A.CallTo(() => userResolver.FindByIdAsync(A<string>.Ignored))
                .Returns(Task.FromResult(A.Dummy<IUser>()));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_should_throw_exception_if_null_user_not_found()
        {
            CreateApp();

            var context = CreateContextForCommand(new AssignContributor { ContributorId = contributorId });

            A.CallTo(() => userResolver.FindByIdAsync(contributorId))
                .Returns(Task.FromResult<IUser>(null));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_should_assign_if_user_found()
        {
            A.CallTo(() => appPlansProvider.GetPlan(null))
                .Returns(new ConfigAppLimitsPlan { MaxContributors = -1 });

            CreateApp();

            var context = CreateContextForCommand(new AssignContributor { ContributorId = contributorId });

            A.CallTo(() => userResolver.FindByIdAsync(contributorId))
                .Returns(Task.FromResult(A.Dummy<IUser>()));

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task RemoveContributor_should_update_domain_object()
        {
            CreateApp()
                .AssignContributor(CreateCommand(new AssignContributor { ContributorId = contributorId }));

            var context = CreateContextForCommand(new RemoveContributor { ContributorId = contributorId });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task AttachClient_should_update_domain_object()
        {
            CreateApp();

            var context = CreateContextForCommand(new AttachClient { Id = clientName });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task ChangePlan_should_throw_if_plan_not_found()
        {
            A.CallTo(() => appPlansProvider.IsConfiguredPlan("my-plan"))
                .Returns(false);

            CreateApp()
                .AttachClient(CreateCommand(new AttachClient { Id = clientName }));

            var context = CreateContextForCommand(new ChangePlan { PlanId = "my-plan" });

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task RenameClient_should_update_domain_object()
        {
            CreateApp()
                .AttachClient(CreateCommand(new AttachClient { Id = clientName }));

            var context = CreateContextForCommand(new UpdateClient { Id = clientName, Name = "New Name" });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task RevokeClient_should_update_domain_object()
        {
            CreateApp()
                .AttachClient(CreateCommand(new AttachClient { Id = clientName }));

            var context = CreateContextForCommand(new RevokeClient { Id = clientName });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task ChangePlan_should_update_domain_object()
        {
            A.CallTo(() => appPlansProvider.IsConfiguredPlan("my-plan"))
                .Returns(true);

            CreateApp();

            var context = CreateContextForCommand(new ChangePlan { PlanId = "my-plan" });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(User.Identifier, app.Id, app.Name, "my-plan")).MustHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_should_not_make_update_for_redirect_result()
        {
            A.CallTo(() => appPlansProvider.IsConfiguredPlan("my-plan"))
                .Returns(true);

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(User.Identifier, app.Id, app.Name, "my-plan"))
                .Returns(CreateRedirectResult());

            CreateApp();

            var context = CreateContextForCommand(new ChangePlan { PlanId = "my-plan" });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Null(app.PlanId);
        }

        [Fact]
        public async Task ChangePlan_should_not_call_billing_manager_for_callback()
        {
            A.CallTo(() => appPlansProvider.IsConfiguredPlan("my-plan"))
                .Returns(true);

            CreateApp();

            var context = CreateContextForCommand(new ChangePlan { PlanId = "my-plan", FromCallback = true });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(User.Identifier, app.Id, app.Name, "my-plan")).MustNotHaveHappened();
        }

        [Fact]
        public async Task AddLanguage_should_update_domain_object()
        {
            CreateApp();

            var context = CreateContextForCommand(new AddLanguage { Language = language });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task RemoveLanguage_should_update_domain_object()
        {
            CreateApp()
                .AddLanguage(CreateCommand(new AddLanguage { Language = language }));

            var context = CreateContextForCommand(new RemoveLanguage { Language = language });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task UpdateLanguage_should_update_domain_object()
        {
            CreateApp()
                .AddLanguage(CreateCommand(new AddLanguage { Language = language }));

            var context = CreateContextForCommand(new UpdateLanguage { Language = language });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        private AppDomainObject CreateApp()
        {
            app.Create(CreateCommand(new CreateApp { Name = AppName }));

            return app;
        }

        private static Task<IChangePlanResult> CreateRedirectResult()
        {
            return Task.FromResult<IChangePlanResult>(new RedirectToCheckoutResult(new Uri("http://squidex.io")));
        }
    }
}
