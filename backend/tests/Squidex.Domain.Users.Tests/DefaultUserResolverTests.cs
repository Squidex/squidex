// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Users
{
    public class DefaultUserResolverTests
    {
        private readonly IUserService userService = A.Fake<IUserService>();
        private readonly DefaultUserResolver sut;

        public DefaultUserResolverTests()
        {
            var serviceProvider = A.Fake<IServiceProvider>();

            var scope = A.Fake<IServiceScope>();

            var scopeFactory = A.Fake<IServiceScopeFactory>();

            A.CallTo(() => scopeFactory.CreateScope())
                .Returns(scope);

            A.CallTo(() => scope.ServiceProvider)
                .Returns(serviceProvider);

            A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory)))
                .Returns(scopeFactory);

            A.CallTo(() => serviceProvider.GetService(typeof(IUserService)))
                .Returns(userService);

            sut = new DefaultUserResolver(serviceProvider);
        }

        [Fact]
        public async Task Should_create_user_and_return_true_if_created()
        {
            var email = "123@email.com";

            var user = A.Fake<IUser>();

            A.CallTo(() => userService.CreateAsync(email, A<UserValues>.That.Matches(x => x.Invited == true), false))
                .Returns(user);

            var result = await sut.CreateUserIfNotExistsAsync(email, true);

            Assert.Equal((user, true), result);
        }

        [Fact]
        public async Task Should_create_user_and_return_false_if_exception_thrown()
        {
            var email = "123@email.com";

            var user = A.Fake<IUser>();

            A.CallTo(() => userService.CreateAsync(email, A<UserValues>._, false))
                .Throws(new InvalidOperationException());

            A.CallTo(() => userService.FindByEmailAsync(email))
                .Returns(user);

            var result = await sut.CreateUserIfNotExistsAsync(email, true);

            Assert.Equal((user, false), result);
        }

        [Fact]
        public async Task Should_add_claim_if_not_added_yet()
        {
            var id = "123";

            await sut.SetClaimAsync(id, "my-claim", "my-value", false);

            A.CallTo(() => userService.UpdateAsync(id,
                    A<UserValues>.That.Matches(x => x.CustomClaims!.Any(y => y.Type == "my-claim" && y.Value == "my-value")), false))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_add_claim_if_not_added_yet_silently()
        {
            var id = "123";

            await sut.SetClaimAsync(id, "my-claim", "my-value", true);

            A.CallTo(() => userService.UpdateAsync(id,
                    A<UserValues>.That.Matches(x => x.CustomClaims!.Any(y => y.Type == "my-claim" && y.Value == "my-value")), true))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_resolve_user_by_email()
        {
            var id = "123@email.com";

            var user = A.Fake<IUser>();

            A.CallTo(() => userService.FindByEmailAsync(id))
                .Returns(user);

            var result = await sut.FindByIdOrEmailAsync(id);

            Assert.Equal(user, result);
        }

        [Fact]
        public async Task Should_resolve_user_by_id()
        {
            var id = "123";

            var user = A.Fake<IUser>();

            A.CallTo(() => userService.FindByIdAsync(id))
                .Returns(user);

            var result = await sut.FindByIdOrEmailAsync(id);

            Assert.Equal(user, result);
        }

        [Fact]
        public async Task Should_resolve_user_by_id_only()
        {
            var id = "123";

            var user = A.Fake<IUser>();

            A.CallTo(() => userService.FindByIdAsync(id))
                .Returns(user);

            var result = await sut.FindByIdOrEmailAsync(id);

            Assert.Equal(user, result);
        }

        [Fact]
        public async Task Should_query_many_by_email()
        {
            var email = "hello@squidex.io";

            var users = ResultList.CreateFrom(0, A.Fake<IUser>());

            A.CallTo(() => userService.QueryAsync(email, 10, 0))
                .Returns(users);

            var result = await sut.QueryByEmailAsync(email);

            Assert.Single(result);
        }

        [Fact]
        public async Task Should_query_by_ids()
        {
            var ids = new[] { "1", "2" };

            var users = ResultList.CreateFrom(0, A.Fake<IUser>());

            A.CallTo(() => userService.QueryAsync(ids))
                .Returns(users);

            var result = await sut.QueryManyAsync(ids);

            Assert.Single(result);
        }

        [Fact]
        public async Task Should_query_all()
        {
            var users = ResultList.CreateFrom(0, A.Fake<IUser>());

            A.CallTo(() => userService.QueryAsync(null, int.MaxValue, 0))
                .Returns(users);

            var result = await sut.QueryAllAsync();

            Assert.Single(result);
        }
    }
}
