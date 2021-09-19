// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class UserMapping : IUserMapping
    {
        private const string UsersFile = "Users.json";
        private readonly Dictionary<string, RefToken> userMap = new Dictionary<string, RefToken>();
        private readonly RefToken initiator;

        public RefToken Initiator
        {
            get => initiator;
        }

        public UserMapping(RefToken initiator)
        {
            Guard.NotNull(initiator, nameof(initiator));

            this.initiator = initiator;
        }

        public void Backup(RefToken token)
        {
            Guard.NotNull(token, nameof(token));

            if (!token.IsUser)
            {
                return;
            }

            userMap[token.Identifier] = token;
        }

        public void Backup(string userId)
        {
            Guard.NotNullOrEmpty(userId, nameof(userId));

            if (!userMap.ContainsKey(userId))
            {
                userMap[userId] = RefToken.User(userId);
            }
        }

        public async Task StoreAsync(IBackupWriter writer, IUserResolver userResolver,
            CancellationToken ct = default)
        {
            Guard.NotNull(writer, nameof(writer));
            Guard.NotNull(userResolver, nameof(userResolver));

            var users = await userResolver.QueryManyAsync(userMap.Keys.ToArray(), ct);

            var json = users.ToDictionary(x => x.Key, x => x.Value.Email);

            await writer.WriteJsonAsync(UsersFile, json, ct);
        }

        public async Task RestoreAsync(IBackupReader reader, IUserResolver userResolver,
            CancellationToken ct = default)
        {
            Guard.NotNull(reader, nameof(reader));
            Guard.NotNull(userResolver, nameof(userResolver));

            var json = await reader.ReadJsonAsync<Dictionary<string, string>>(UsersFile, ct);

            foreach (var (userId, email) in json)
            {
                var (user, _) = await userResolver.CreateUserIfNotExistsAsync(email, false, ct);

                if (user != null)
                {
                    userMap[userId] = RefToken.User(user.Id);
                }
            }
        }

        public bool TryMap(string userId, out RefToken result)
        {
            Guard.NotNullOrEmpty(userId, nameof(userId));

            result = initiator;

            if (userMap.TryGetValue(userId, out var mapped))
            {
                result = mapped;
                return true;
            }

            return false;
        }

        public bool TryMap(RefToken token, out RefToken result)
        {
            Guard.NotNull(token, nameof(token));

            result = initiator;

            if (token.IsClient)
            {
                result = token;
                return true;
            }

            if (userMap.TryGetValue(token.Identifier, out var mapped))
            {
                result = mapped;
                return true;
            }

            return false;
        }
    }
}
