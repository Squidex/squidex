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
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.Apps.Services.Implementations;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppCommandMiddlewareTests : HandlerTestBase<AppDomainObject>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAppPlansProvider appPlansProvider = A.Fake<IAppPlansProvider>();
        private readonly IAppPlanBillingManager appPlansBillingManager = A.Fake<IAppPlanBillingManager>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly Language language = Language.DE;
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientName = "client";
        private readonly Guid patternId = Guid.NewGuid();
        private readonly AppDomainObject app = new AppDomainObject(new InitialPatterns());
        private readonly AppCommandMiddleware sut;

        protected override Guid Id
        {
            get { return AppId; }
        }

        public AppCommandMiddlewareTests()
        {
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
        public async Task AssignContributor_should_update_domain_object_if_user_found()
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

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(User.Identifier, AppId, AppName, "my-plan"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_should_not_make_update_for_redirect_result()
        {
            A.CallTo(() => appPlansProvider.IsConfiguredPlan("my-plan"))
                .Returns(true);

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(User.Identifier, AppId, AppName, "my-plan"))
                .Returns(CreateRedirectResult());

            CreateApp();

            var context = CreateContextForCommand(new ChangePlan { PlanId = "my-plan" });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Null(app.State.Plan);
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

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(User.Identifier, AppId, AppName, "my-plan"))
                .MustNotHaveHappened();
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

        [Fact]
        public async Task AddPattern_should_update_domain_object()
        {
            CreateApp();

            var context = CreateContextForCommand(new AddPattern { Name = "Any", Pattern = ".*" });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task UpdatePattern_should_update_domain()
        {
            CreateApp()
                .AddPattern(CreateCommand(new AddPattern { Id = patternId, Name = "Any", Pattern = "." }));

            var context = CreateContextForCommand(new UpdatePattern { Id = patternId, Name = "Number", Pattern = "[0-9]" });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task DeletePattern_should_update_domain_object()
        {
            CreateApp()
                .AddPattern(CreateCommand(new AddPattern { Id = patternId, Name = "Any", Pattern = "." }));

            var context = CreateContextForCommand(new DeletePattern { Id = patternId });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        private AppDomainObject CreateApp()
        {
            app.Create(CreateCommand(new CreateApp { AppId = AppId, Name = AppName }));

            return app;
        }

        private static Task<IChangePlanResult> CreateRedirectResult()
        {
            return Task.FromResult<IChangePlanResult>(new RedirectToCheckoutResult(new Uri("http://squidex.io")));
        }
    }
}
