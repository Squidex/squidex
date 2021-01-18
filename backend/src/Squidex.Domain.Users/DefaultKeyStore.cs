// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Users
{
    public sealed class DefaultKeyStore : ISigningCredentialStore, IValidationKeysStore
    {
        private readonly ISnapshotStore<State, Guid> store;
        private SigningCredentials? cachedKey;
        private SecurityKeyInfo[]? cachedKeyInfo;

        [CollectionName("Identity_Keys")]
        public sealed class State
        {
            public string Key { get; set; }

            public RSAParameters Parameters { get; set; }
        }

        public DefaultKeyStore(ISnapshotStore<State, Guid> store)
        {
            this.store = store;
        }

        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            var (_, key) = await GetOrCreateKeyAsync();

            return key;
        }

        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var (info, _) = await GetOrCreateKeyAsync();

            return info;
        }

        private async Task<(SecurityKeyInfo[], SigningCredentials)> GetOrCreateKeyAsync()
        {
            if (cachedKey != null && cachedKeyInfo != null)
            {
                return (cachedKeyInfo, cachedKey);
            }

            var (state, _) = await store.ReadAsync(default);

            RsaSecurityKey securityKey;

            if (state == null)
            {
                securityKey = new RsaSecurityKey(RSA.Create(2048))
                {
                    KeyId = CryptoRandom.CreateUniqueId(16)
                };

                state = new State { Key = securityKey.KeyId };

                if (securityKey.Rsa != null)
                {
                    var parameters = securityKey.Rsa.ExportParameters(includePrivateParameters: true);

                    state.Parameters = parameters;
                }
                else
                {
                    state.Parameters = securityKey.Parameters;
                }

                try
                {
                    await store.WriteAsync(default, state, 0, 0);

                    return CreateCredentialsPair(securityKey);
                }
                catch (InconsistentStateException)
                {
                    (state, _) = await store.ReadAsync(default);
                }
            }

            if (state == null)
            {
                throw new InvalidOperationException("Cannot read key.");
            }

            securityKey = new RsaSecurityKey(state.Parameters)
            {
                KeyId = state.Key
            };

            return CreateCredentialsPair(securityKey);
        }

        private (SecurityKeyInfo[], SigningCredentials) CreateCredentialsPair(RsaSecurityKey securityKey)
        {
            cachedKey = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            cachedKeyInfo = new[]
            {
                new SecurityKeyInfo { Key = cachedKey.Key, SigningAlgorithm = cachedKey.Algorithm }
            };

            return (cachedKeyInfo, cachedKey);
        }
    }
}
