// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class UserMappingTests
    {
        private readonly RefToken initiator = Subject("me");
        private readonly UserMapping sut;

        public UserMappingTests()
        {
            sut = new UserMapping(initiator);
        }

        [Fact]
        public async Task Should_backup_users_but_no_clients()
        {
            sut.Backup("user1");
            sut.Backup(Subject("user2"));

            sut.Backup(Client("client"));

            var user1 = CreateUser("user1", "mail1@squidex.io");
            var user2 = CreateUser("user2", "mail2@squidex.io");

            var users = new Dictionary<string, IUser>
            {
                [user1.Id] = user1,
                [user2.Id] = user2
            };

            var userResolver = A.Fake<IUserResolver>();

            A.CallTo(() => userResolver.QueryManyAsync(A<string[]>.That.Is(user1.Id, user2.Id)))
                .Returns(users);

            var writer = A.Fake<IBackupWriter>();

            Dictionary<string, string>? storedUsers = null;

            A.CallTo(() => writer.WriteJsonAsync(A<string>._, A<object>._))
                .Invokes((string _, object json) => storedUsers = (Dictionary<string, string>)json);

            await sut.StoreAsync(writer, userResolver);

            Assert.Equal(new Dictionary<string, string>
            {
                [user1.Id] = user1.Email,
                [user2.Id] = user2.Email
            }, storedUsers);
        }

        [Fact]
        public async Task Should_restore_users()
        {
            var user1 = CreateUser("user1", "mail1@squidex.io");
            var user2 = CreateUser("user2", "mail2@squidex.io");

            var reader = SetupReader(user1, user2);

            var userResolver = A.Fake<IUserResolver>();

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user1.Email, false))
                .Returns((user1, false));

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user2.Email, false))
                .Returns((user2, true));

            await sut.RestoreAsync(reader, userResolver);

            Assert.True(sut.TryMap("user1_old", out var mapped1));
            Assert.True(sut.TryMap(Subject("user2_old"), out var mapped2));

            Assert.Equal(Subject("user1"), mapped1);
            Assert.Equal(Subject("user2"), mapped2);
        }

        [Fact]
        public void Should_return_initiator_if_user_not_found()
        {
            var user = Subject("user1");

            Assert.False(sut.TryMap(user, out var mapped));
            Assert.Same(initiator, mapped);
        }

        [Fact]
        public void Should_create_same_token_if_mapping_client()
        {
            var client = Client("client1");

            Assert.True(sut.TryMap(client, out var mapped));
            Assert.Same(client, mapped);
        }

        private static IUser CreateUser(string id, string email)
        {
            var user = A.Fake<IUser>();

            A.CallTo(() => user.Id).Returns(id);
            A.CallTo(() => user.Email).Returns(email);

            return user;
        }

        private static IBackupReader SetupReader(params IUser[] users)
        {
            var storedUsers = users.ToDictionary(x => $"{x.Id}_old", x => x.Email);

            var reader = A.Fake<IBackupReader>();

            A.CallTo(() => reader.ReadJsonAsync<Dictionary<string, string>>(A<string>._))
                .Returns(storedUsers);

            return reader;
        }

        private static RefToken Client(string identifier)
        {
            return new RefToken(RefTokenType.Client, identifier);
        }

        private static RefToken Subject(string identifier)
        {
            return new RefToken(RefTokenType.Subject, identifier);
        }
    }
}
