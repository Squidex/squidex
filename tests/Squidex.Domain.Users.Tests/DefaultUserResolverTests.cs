// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Squidex.Domain.Users
{
    public class DefaultUserResolverTests
    {
        private readonly UserManager<IdentityUser> userManager = A.Fake<UserManager<IdentityUser>>();
        private readonly DefaultUserResolver sut;

        public DefaultUserResolverTests()
        {
            var userFactory = A.Fake<IUserFactory>();

            A.CallTo(() => userFactory.IsId(A<string>.That.StartsWith("id")))
                .Returns(true);

            A.CallTo(() => userManager.NormalizeKey(A<string>.Ignored))
                .ReturnsLazily(c => c.GetArgument<string>(0).ToUpperInvariant());

            sut = new DefaultUserResolver(userManager, userFactory);
        }

        [Fact]
        public async Task Should_resolve_user_by_email()
        {
            var (user, claims) = GernerateUser("id1");

            A.CallTo(() => userManager.FindByEmailAsync(user.Email))
                .Returns(user);

            A.CallTo(() => userManager.GetClaimsAsync(user))
                .Returns(claims);

            var result = await sut.FindByIdOrEmailAsync(user.Email);

            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);

            Assert.Equal(claims, result.Claims);
        }

        [Fact]
        public async Task Should_resolve_user_by_id1()
        {
            var (user, claims) = GernerateUser("id2");

            A.CallTo(() => userManager.FindByIdAsync(user.Id))
                .Returns(user);

            A.CallTo(() => userManager.GetClaimsAsync(user))
                .Returns(claims);

            var result = await sut.FindByIdOrEmailAsync(user.Id);

            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);

            Assert.Equal(claims, result.Claims);
        }

        [Fact]
        public async Task Should_query_many_by_email_async()
        {
            var (user1, claims1) = GernerateUser("id1");
            var (user2, claims2) = GernerateUser("id2");

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

        private static (IdentityUser, List<Claim>) GernerateUser(string id)
        {
            var user = new IdentityUser { Id = id, Email = $"email_{id}", NormalizedEmail = $"EMAIL_{id}" };

            var claims = new List<Claim>
            {
                new Claim($"{id}_a", "1"),
                new Claim($"{id}_b", "2")
            };

            return (user, claims);
        }
    }
}
