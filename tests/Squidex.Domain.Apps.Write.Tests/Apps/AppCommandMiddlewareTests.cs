// ==========================================================================
//  AppCommandMiddlewareTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Apps.Services.Implementations;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppCommandMiddlewareTests : HandlerTestBase<AppDomainObject>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAppPlansProvider appPlansProvider = A.Fake<IAppPlansProvider>();
        private readonly IAppPlanBillingManager appPlansBillingManager = A.Fake<IAppPlanBillingManager>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly AppCommandMiddleware sut;
        private readonly AppDomainObject app;
        private readonly Language language = Language.DE;
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientName = "client";

        public AppCommandMiddlewareTests()
        {
            app = new AppDomainObject(AppId, -1);

            A.CallTo(() => appProvider.GetAppAsync(AppName))
                .Returns((IAppEntity)null);

            A.CallTo(() => userResolver.FindByIdAsync(contributorId))
                .Returns(A.Fake<IUser>());

            sut = new AppCommandMiddleware(Handler, appProvider, appPlansProvider, appPlansBillingManager, userResolver);
        }

        [Fact]
        public async Task Create_should_create_domain_object()
        {
            var context = CreateContextForCommand(new CreateApp { Name = AppName, AppId = AppId });

            await TestCreate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(AppId, context.Result<EntityCreatedResult<Guid>>().IdOrValue);
        }

        [Fact]
        public async Task AssignContributor_should_assign_if_user_found()
        {
            A.CallTo(() => appPlansProvider.GetPlan(null))
                .Returns(new ConfigAppLimitsPlan { MaxContributors = -1 });

            CreateApp();

            var context = CreateContextForCommand(new AssignContributor { ContributorId = contributorId });

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

            Assert.Null(app.Plan);
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
