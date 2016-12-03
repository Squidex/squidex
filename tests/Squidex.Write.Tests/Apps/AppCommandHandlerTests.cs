// ==========================================================================
//  AppCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Users;
using Squidex.Read.Users.Repositories;
using Squidex.Write.Apps.Commands;
using Squidex.Write.Utils;
using Xunit;

// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Apps
{
    public class AppCommandHandlerTests : HandlerTestBase<AppDomainObject>
    {
        private readonly Mock<ClientKeyGenerator> keyGenerator = new Mock<ClientKeyGenerator>();
        private readonly Mock<IAppRepository> appRepository = new Mock<IAppRepository>();
        private readonly Mock<IUserRepository> userRepository = new Mock<IUserRepository>();
        private readonly AppCommandHandler sut;
        private readonly AppDomainObject app;
        private readonly UserToken subjectId = new UserToken("subject", Guid.NewGuid().ToString());
        private readonly DateTime expiresUtc = DateTime.UtcNow.AddYears(1);
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientSecret = Guid.NewGuid().ToString();
        private readonly string clientName = "client";
        private readonly string appName = "my-app";

        public AppCommandHandlerTests()
        {
            app = new AppDomainObject(Id, 0);

            sut = new AppCommandHandler(Handler, appRepository.Object, userRepository.Object, keyGenerator.Object);
        }

        [Fact]
        public async Task Create_should_throw_if_a_name_with_same_name_already_exists()
        {
            var command = new CreateApp { Name = appName, AggregateId = Id, User = subjectId };
            var context = new CommandContext(command);

            appRepository.Setup(x => x.FindAppByNameAsync(appName)).Returns(Task.FromResult(new Mock<IAppEntity>().Object)).Verifiable();

            await TestCreate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(async () => await sut.HandleAsync(context));
            }, false);

            appRepository.VerifyAll();
        }

        [Fact]
        public async Task Create_should_create_app_if_name_is_free()
        {
            var command = new CreateApp { Name = appName, AggregateId = Id, User = subjectId };
            var context = new CommandContext(command);

            appRepository.Setup(x => x.FindAppByNameAsync(appName)).Returns(Task.FromResult<IAppEntity>(null)).Verifiable();

            await TestCreate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(command.AggregateId, context.Result<Guid>());
        }

        [Fact]
        public async Task AssignContributor_should_throw_if_user_not_found()
        {
            CreateApp();

            var command = new AssignContributor { AggregateId = Id, ContributorId = contributorId };
            var context = new CommandContext(command);

            userRepository.Setup(x => x.FindUserByIdAsync(command.ContributorId)).Returns(Task.FromResult<IUserEntity>(null));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_should_throw_if_null_user_not_found()
        {
            CreateApp();

            var command = new AssignContributor { AggregateId = Id, ContributorId = null };
            var context = new CommandContext(command);

            userRepository.Setup(x => x.FindUserByIdAsync(command.ContributorId)).Returns(Task.FromResult<IUserEntity>(null));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_should_assign_if_user_found()
        {
            CreateApp();

            var command = new AssignContributor { AggregateId = Id, ContributorId = contributorId };
            var context = new CommandContext(command);

            userRepository.Setup(x => x.FindUserByIdAsync(command.ContributorId)).Returns(Task.FromResult(new Mock<IUserEntity>().Object));

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task RemoveContributor_should_update_domain_object()
        {
            CreateApp()
                .AssignContributor(new AssignContributor { ContributorId = contributorId });

            var command = new RemoveContributor { AggregateId = Id, ContributorId = contributorId };
            var context = new CommandContext(command);

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task AttachClient_should_update_domain_object()
        {
            keyGenerator.Setup(x => x.GenerateKey()).Returns(clientSecret).Verifiable();

            CreateApp();

            var timestamp = DateTime.Today;

            var command = new AttachClient { Id = clientName, AggregateId = Id, Timestamp = timestamp };
            var context = new CommandContext(command);

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });

            keyGenerator.VerifyAll();

            context.Result<AppClient>().ShouldBeEquivalentTo(
                new AppClient(clientName, clientSecret, timestamp.AddYears(1)));
        }

        [Fact]
        public async Task RenameClient_should_update_domain_object()
        {
            CreateApp()
                .AttachClient(new AttachClient { Id = clientName }, clientSecret, expiresUtc);

            var command = new RenameClient { AggregateId = Id, Id = clientName, Name = "New Name" };
            var context = new CommandContext(command);

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task RevokeClient_should_update_domain_object()
        {
            CreateApp()
                .AttachClient(new AttachClient { Id = clientName }, clientSecret, expiresUtc);

            var command = new RevokeClient { AggregateId = Id, Id = clientName };
            var context = new CommandContext(command);

            await TestUpdate(app, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        private AppDomainObject CreateApp()
        {
            app.Create(new CreateApp { Name = appName, User = subjectId });

            return app;
        }
    }
}
