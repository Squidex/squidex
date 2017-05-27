// ==========================================================================
//  AppCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Apps.Services;
using Squidex.Read.Apps.Services.Implementations;
using Squidex.Read.Users;
using Squidex.Write.Apps.Commands;
using Squidex.Write.TestHelpers;
using Xunit;

// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Apps
{
    public class AppCommandHandlerTests : HandlerTestBase<AppDomainObject>
    {
        private readonly Mock<ClientKeyGenerator> keyGenerator = new Mock<ClientKeyGenerator>();
        private readonly Mock<IAppRepository> appRepository = new Mock<IAppRepository>();
        private readonly Mock<IAppLimitsProvider> appLimitsProvider = new Mock<IAppLimitsProvider>();
        private readonly Mock<IUserResolver> userResolver = new Mock<IUserResolver>();
        private readonly AppCommandHandler sut;
        private readonly AppDomainObject app;
        private readonly Language language = Language.DE;
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientSecret = Guid.NewGuid().ToString();
        private readonly string clientName = "client";

        public AppCommandHandlerTests()
        {
            app = new AppDomainObject(AppId, -1);

            sut = new AppCommandHandler(Handler, appRepository.Object, appLimitsProvider.Object, userResolver.Object, keyGenerator.Object);
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

            userResolver.Setup(x => x.FindById(contributorId)).Returns(Task.FromResult<IUser>(null));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_throw_exception_if_reached_max_contributor_size()
        {
            appLimitsProvider.Setup(x => x.GetPlan(0)).Returns(new ConfigAppLimitsPlan { MaxContributors = 2 });

            CreateApp()
                .AssignContributor(CreateCommand(new AssignContributor { ContributorId = "1" }))
                .AssignContributor(CreateCommand(new AssignContributor { ContributorId = "2" }));

            var context = CreateContextForCommand(new AssignContributor { ContributorId = contributorId });

            userResolver.Setup(x => x.FindById(It.IsAny<string>())).Returns(Task.FromResult(new Mock<IUser>().Object));

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

            userResolver.Setup(x => x.FindById(contributorId)).Returns(Task.FromResult<IUser>(null));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_should_assign_if_user_found()
        {
            appLimitsProvider.Setup(x => x.GetPlan(0)).Returns(new ConfigAppLimitsPlan { MaxContributors = -1 });

            CreateApp();

            var context = CreateContextForCommand(new AssignContributor { ContributorId = contributorId });

            userResolver.Setup(x => x.FindById(contributorId)).Returns(Task.FromResult(new Mock<IUser>().Object));

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
            keyGenerator.Setup(x => x.GenerateKey())
                .Returns(clientSecret)
                .Verifiable();

            CreateApp();

            var context = CreateContextForCommand(new AttachClient { Id = clientName });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            keyGenerator.VerifyAll();

            context.Result<EntityCreatedResult<AppClient>>().IdOrValue.ShouldBeEquivalentTo(new AppClient(clientName, clientSecret));
        }

        [Fact]
        public async Task RenameClient_should_update_domain_object()
        {
            CreateApp()
                .AttachClient(CreateCommand(new AttachClient { Id = clientName }), clientSecret);

            var context = CreateContextForCommand(new RenameClient { Id = clientName, Name = "New Name" });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task RevokeClient_should_update_domain_object()
        {
            CreateApp()
                .AttachClient(CreateCommand(new AttachClient { Id = clientName }), clientSecret);

            var context = CreateContextForCommand(new RevokeClient { Id = clientName });

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
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
