﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class UserMapping : IUserMapping
    {
        private const string UsersFile = "Users.json";
        private readonly Dictionary<string, RefToken> userMap = new Dictionary<string, RefToken>();
        private readonly RefToken initiator;

        public RefToken Initiator
        {
            get { return initiator; }
        }

        public UserMapping(RefToken initiator)
        {
            Guard.NotNull(initiator);

            this.initiator = initiator;
        }

        public void Backup(RefToken token)
        {
            Guard.NotNull(userMap);

            if (!token.IsSubject)
            {
                return;
            }

            userMap[token.Identifier] = token;
        }

        public void Backup(string userId)
        {
            Guard.NotNullOrEmpty(userId);

            if (!userMap.ContainsKey(userId))
            {
                userMap[userId] = new RefToken(RefTokenType.Subject, userId);
            }
        }

        public async Task StoreAsync(IBackupWriter writer, IUserResolver userResolver)
        {
            Guard.NotNull(writer);
            Guard.NotNull(userResolver);

            var users = await userResolver.QueryManyAsync(userMap.Keys.ToArray());

            var json = users.ToDictionary(x => x.Key, x => x.Value.Email);

            await writer.WriteJsonAsync(UsersFile, json);
        }

        public async Task RestoreAsync(IBackupReader reader, IUserResolver userResolver)
        {
            Guard.NotNull(reader);
            Guard.NotNull(userResolver);

            var json = await reader.ReadJsonAttachmentAsync<Dictionary<string, string>>(UsersFile);

            foreach (var (userId, email) in json)
            {
                var user = await userResolver.FindByIdOrEmailAsync(email);

                if (user == null && await userResolver.CreateUserIfNotExistsAsync(email, false))
                {
                    user = await userResolver.FindByIdOrEmailAsync(email);
                }

                if (user != null)
                {
                    userMap[userId] = new RefToken(RefTokenType.Subject, user.Id);
                }
            }
        }

        public bool TryMap(string userId, out RefToken result)
        {
            Guard.NotNullOrEmpty(userId);

            if (userMap.TryGetValue(userId, out var mapped))
            {
                result = mapped;
                return true;
            }

            result = initiator;
            return false;
        }

        public bool TryMap(RefToken token, out RefToken result)
        {
            Guard.NotNull(token);

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

            result = initiator;
            return false;
        }
    }
}
