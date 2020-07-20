// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Squidex.Domain.Users
{
    public class DefaultUserResolverTests
    {
        private readonly IUserFactory userFactory = A.Fake<IUserFactory>();
        private readonly UserManager<IdentityUser> userManager = A.Fake<UserManager<IdentityUser>>();
        private readonly DefaultUserResolver sut;

        public DefaultUserResolverTests()
        {
            A.CallTo(() => userFactory.IsId(A<string>.That.StartsWith("id")))
                .Returns(true);

            A.CallTo(() => userManager.NormalizeEmail(A<string>._))
                .ReturnsLazily(c => c.GetArgument<string>(0)!.ToUpperInvariant());

            var serviceProvider = A.Fake<IServiceProvider>();

            var scope = A.Fake<IServiceScope>();

            var scopeFactory = A.Fake<IServiceScopeFactory>();

            A.CallTo(() => scopeFactory.CreateScope())
                .Returns(scope);

            A.CallTo(() => scope.ServiceProvider)
                .Returns(serviceProvider);

            A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory)))
                .Returns(scopeFactory);

            A.CallTo(() => serviceProvider.GetService(typeof(IUserFactory)))
                .Returns(userFactory);

            A.CallTo(() => serviceProvider.GetService(typeof(UserManager<IdentityUser>)))
                .Returns(userManager);

            sut = new DefaultUserResolver(serviceProvider);
        }

        [Fact]
        public async Task Should_create_user_and_return_true_when_created()
        {
            var (user, claims) = GenerateUser("id1");

            A.CallTo(() => userFactory.Create(user.Email))
                .Returns(user);

            A.CallTo(() => userManager.CreateAsync(user))
                .Returns(IdentityResult.Success);

            SetupUser(user, claims);

            var (result, created) = await sut.CreateUserIfNotExistsAsync(user.Email, false);

            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.Email, result!.Email);

            Assert.True(created);
        }

        [Fact]
        public async Task Should_create_user_and_return_false_when_already_exists()
        {
            var (user, claims) = GenerateUser("id1");

            A.CallTo(() => userFactory.Create(user.Email))
                .Returns(user);

            A.CallTo(() => userManager.CreateAsync(user))
                .Returns(IdentityResult.Failed());

            SetupUser(user, claims);

            var (result, created) = await sut.CreateUserIfNotExistsAsync(user.Email, false);

            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.Email, result!.Email);

            Assert.False(created);
        }

        [Fact]
        public async Task Should_create_user_and_return_false_when_exception_thrown()
        {
            var (user, claims) = GenerateUser("id1");

            A.CallTo(() => userFactory.Create(user.Email))
                .Throws(new InvalidOperationException());

            A.CallTo(() => userManager.CreateAsync(user))
                .Returns(IdentityResult.Failed());

            SetupUser(user, claims);

            var (result, created) = await sut.CreateUserIfNotExistsAsync(user.Email, false);

            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.Email, result!.Email);

            Assert.False(created);
        }

        [Fact]
        public async Task Should_add_claim_when_not_added_yet()
        {
            var (user, claims) = GenerateUser("id2");

            A.CallTo(() => userManager.AddClaimsAsync(user, A<IEnumerable<Claim>>._))
                .Returns(IdentityResult.Success);

            SetupUser(user, claims);

            await sut.SetClaimAsync("id2", "my-claim", "new-value");

            A.CallTo(() => userManager.AddClaimsAsync(user,
                    A<IEnumerable<Claim>>.That.Matches(x => x.Any(y => y.Type == "my-claim" && y.Value == "new-value"))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_previous_claim()
        {
            var (user, claims) = GenerateUser("id2");

            claims.Add(new Claim("my-claim", "old-value"));

            A.CallTo(() => userManager.AddClaimsAsync(user, A<IEnumerable<Claim>>._))
                .Returns(IdentityResult.Success);

            A.CallTo(() => userManager.RemoveClaimsAsync(user, A<IEnumerable<Claim>>._))
                .Returns(IdentityResult.Success);

            SetupUser(user, claims);

            await sut.SetClaimAsync("id2", "my-claim", "new-value");

            A.CallTo(() => userManager.AddClaimsAsync(user,
                    A<IEnumerable<Claim>>.That.Matches(x => x.Any(y => y.Type == "my-claim" && y.Value == "new-value"))))
                .MustHaveHappened();

            A.CallTo(() => userManager.RemoveClaimsAsync(user,
                    A<IEnumerable<Claim>>.That.Matches(x => x.Any(y => y.Type == "my-claim" && y.Value == "old-value"))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_resolve_user_by_email()
        {
            var (user, claims) = GenerateUser("id1");

            SetupUser(user, claims);

            var result = await sut.FindByIdOrEmailAsync(user.Email);

            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.Email, result!.Email);

            Assert.Equal(claims, result!.Claims);
        }

        [Fact]
        public async Task Should_resolve_user_by_id()
        {
            var (user, claims) = GenerateUser("id2");

            SetupUser(user, claims);

            var result = await sut.FindByIdOrEmailAsync(user.Id)!;

            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.Email, result!.Email);

            Assert.Equal(claims, result!.Claims);
        }

        [Fact]
        public async Task Should_resolve_user_by_id_only()
        {
            var (user, claims) = GenerateUser("id2");

            SetupUser(user, claims);

            var result = await sut.FindByIdAsync(user.Id)!;

            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.Email, result!.Email);

            Assert.Equal(claims, result!.Claims);
        }

        [Fact]
        public async Task Should_query_many_by_email_async()
        {
            var (user1, claims1) = GenerateUser("id1");
            var (user2, claims2) = GenerateUser("id2");

            var list = new List<IdentityUser> { user1, user2 };

            A.CallTo(() => userManager.Users)
                .Returns(list.AsQueryable());

            A.CallTo(() => userManager.GetClaimsAsync(user2))
                .Returns(claims2);

            var result = await sut.QueryByEmailAsync("2");

            Assert.Equal(user2.Id, result[0].Id);
            Assert.Equal(user2.Email, result[0].Email);

            Assert.Equal(claims2, result[0].Claims);

            A.CallTo(() => userManager.GetClaimsAsync(user1))
                .MustNotHaveHappened();
        }

        private static (IdentityUser, List<Claim>) GenerateUser(string id)
        {
            var user = new IdentityUser { Id = id, Email = $"email_{id}", NormalizedEmail = $"EMAIL_{id}" };

            var claims = new List<Claim>
            {
                new Claim($"{id}_a", "1"),
                new Claim($"{id}_b", "2")
            };

            return (user, claims);
        }

        private void SetupUser(IdentityUser user, List<Claim> claims)
        {
            A.CallTo(() => userManager.FindByEmailAsync(user.Email))
                .Returns(user);

            A.CallTo(() => userManager.FindByIdAsync(user.Id))
                .Returns(user);

            A.CallTo(() => userManager.GetClaimsAsync(user))
                .Returns(claims);
        }
    }
}
