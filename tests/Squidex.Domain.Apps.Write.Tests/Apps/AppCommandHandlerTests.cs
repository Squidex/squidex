// ==========================================================================
//  AppCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Moq;
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
        private readonly Mock<IAppRepository> appRepository = new Mock<IAppRepository>();
        private readonly Mock<IAppPlansProvider> appPlansProvider = new Mock<IAppPlansProvider>();
        private readonly Mock<IAppPlanBillingManager> appPlansBillingManager = new Mock<IAppPlanBillingManager>();
        private readonly Mock<IUserResolver> userResolver = new Mock<IUserResolver>();
        private readonly AppCommandHandler sut;
        private readonly AppDomainObject app;
        private readonly Language language = Language.DE;
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientName = "client";

        public AppCommandHandlerTests()
        {
            app = new AppDomainObject(AppId, -1);

            sut = new AppCommandHandler(Handler, appRepository.Object, appPlansProvider.Object, appPlansBillingManager.Object, userResolver.Object);
        }

        [Fact]
        public async Task Create_should_throw_exception_if_a_name_with_same_name_already_exists()
        {
            var context = CreateContextForCommand(new CreateApp { Name = AppName, AppId = AppId });

            appRepository.Setup(x => x.FindAppAsync(AppName))
                .Returns(Task.FromResult(new Mock<IAppEntity>().Object))
                .Verifiable();

            await TestCreate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(async () => await sut.HandleAsync(context));
            }, false);

            appRepository.VerifyAll();
        }

        [Fact]
        public async Task Create_should_create_app_if_name_is_free()
        {
            var context = CreateContextForCommand(new CreateApp { Name = AppName, AppId = AppId });

            appRepository.Setup(x => x.FindAppAsync(AppName))
                .Returns(Task.FromResult<IAppEntity>(null))
                .Verifiable();

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

            userResolver.Setup(x => x.FindByIdAsync(contributorId)).Returns(Task.FromResult<IUser>(null));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_throw_exception_if_reached_max_contributor_size()
        {
            appPlansProvider.Setup(x => x.GetPlan(null)).Returns(new ConfigAppLimitsPlan { MaxContributors = 2 });

            CreateApp()
                .AssignContributor(CreateCommand(new AssignContributor { ContributorId = "1" }))
                .AssignContributor(CreateCommand(new AssignContributor { ContributorId = "2" }));

            var context = CreateContextForCommand(new AssignContributor { ContributorId = contributorId });

            userResolver.Setup(x => x.FindByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(new Mock<IUser>().Object));

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

            userResolver.Setup(x => x.FindByIdAsync(contributorId)).Returns(Task.FromResult<IUser>(null));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_should_assign_if_user_found()
        {
            appPlansProvider.Setup(x => x.GetPlan(null)).Returns(new ConfigAppLimitsPlan { MaxContributors = -1 });

            CreateApp();

            var context = CreateContextForCommand(new AssignContributor { ContributorId = contributorId });

            userResolver.Setup(x => x.FindByIdAsync(contributorId)).Returns(Task.FromResult(new Mock<IUser>().Object));

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
            appPlansProvider.Setup(x => x.IsConfiguredPlan("my-plan")).Returns(false);

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
            appPlansProvider.Setup(x => x.IsConfiguredPlan("my-plan")).Returns(true);

            CreateApp();

            var context = CreateContextForCommand(new ChangePlan { PlanId = "my-plan" });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            appPlansBillingManager.Verify(x => x.ChangePlanAsync(User.Identifier, app.Id, app.Name, "my-plan"), Times.Once());
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
    }
}
