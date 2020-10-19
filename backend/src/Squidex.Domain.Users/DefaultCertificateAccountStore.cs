// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using LettuceEncrypt.Accounts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Users
{
    public sealed class DefaultCertificateAccountStore : IAccountStore
    {
        private readonly ISnapshotStore<State, Guid> store;

        [CollectionName("Identity_CertificateAccount")]
        public sealed class State
        {
            public AccountModel Account { get; set; }

            public State()
            {
            }

            public State(AccountModel account)
            {
                Account = account;
            }
        }

        public DefaultCertificateAccountStore(ISnapshotStore<State, Guid> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public async Task<AccountModel?> GetAccountAsync(CancellationToken cancellationToken)
        {
            var (value, _) = await store.ReadAsync(default);

            return value?.Account;
        }

        public Task SaveAccountAsync(AccountModel account, CancellationToken cancellationToken)
        {
            Guard.NotNull(account, nameof(account));

            var state = new State(account);

            return store.WriteAsync(default, state, EtagVersion.Any, 0);
        }
    }
}
