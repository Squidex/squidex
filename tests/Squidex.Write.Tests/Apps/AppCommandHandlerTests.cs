// ==========================================================================
//  AppCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Moq;
using Squidex.Infrastructure;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Users;
using Squidex.Read.Users.Repositories;
using Squidex.Write.Apps;
using Squidex.Write.Apps.Commands;
using Squidex.Write.Tests.Utils;
using Xunit;
// ReSharper disable ImplicitlyCapturedClosure

namespace Squidex.Write.Tests.Apps
{
    public class AppCommandHandlerTests : HandlerTestBase<AppDomainObject>
    {
        private readonly Mock<IAppRepository> appRepository = new Mock<IAppRepository>();
        private readonly Mock<IUserRepository> userRepository = new Mock<IUserRepository>();
        private readonly AppCommandHandler sut;

        public AppCommandHandlerTests()
        {
            sut = new AppCommandHandler(
                DomainObjectFactory.Object, 
                DomainObjectRepository.Object, 
                appRepository.Object,
                userRepository.Object);
        }

        [Fact]
        public async Task Create_should_throw_if_a_name_with_same_name_already_exists()
        {
            appRepository.Setup(x => x.FindAppByNameAsync("my-app")).Returns(Task.FromResult(new Mock<IAppEntity>().Object)).Verifiable();

            await TestCreate(new AppDomainObject(Id, 0), async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(async () => await sut.On(new CreateApp { Name = "my-app" }));
            }, false);

            appRepository.VerifyAll();
        }

        [Fact]
        public async Task Create_should_create_app_if_name_is_free()
        {
            var command = new CreateApp { Name = "my-app", AggregateId = Id, SubjectId = "456" };

            appRepository.Setup(x => x.FindAppByNameAsync("my-app")).Returns(Task.FromResult<IAppEntity>(null)).Verifiable();

            await TestCreate(new AppDomainObject(Id, 0), async _ =>
            {
                await sut.On(command);
            });

            appRepository.VerifyAll();
        }
         
        [Fact]
        public async Task AssignContributor_should_throw_if_user_not_found()
        {
            var app =
                new AppDomainObject(Id, 0)
                    .Create(new CreateApp { Name = "my-app", SubjectId = "123" });

            var command = new AssignContributor { AggregateId = Id, ContributorId = "456" };

            userRepository.Setup(x => x.FindUserByIdAsync(command.ContributorId)).Returns(Task.FromResult<IUserEntity>(null));

            await TestUpdate(app, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.On(command));
            }, false);
        }

        [Fact]
        public async Task AssignContributor_should_assign_if_user_found()
        {
            var app =
                new AppDomainObject(Id, 0)
                    .Create(new CreateApp { Name = "my-app", SubjectId = "123" });

            var command = new AssignContributor { AggregateId = Id, ContributorId = "456" };

            userRepository.Setup(x => x.FindUserByIdAsync(command.ContributorId)).Returns(Task.FromResult(new Mock<IUserEntity>().Object));

            await TestUpdate(app, async _ =>
            {
                await sut.On(command);
            });
        }

        [Fact]
        public async Task RemoveContributor_should_update_domain_object()
        {
            var app =
                new AppDomainObject(Id, 0)
                    .Create(new CreateApp { Name = "my-app", SubjectId = "123" })
                    .AssignContributor(new AssignContributor { ContributorId = "456" });

            var command = new RemoveContributor { AggregateId = Id, ContributorId = "456" };

            await TestUpdate(app, async _ =>
            {
                await sut.On(command);
            });
        }
    }
}
