// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;
using IdentityModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Users;

public sealed class DefaultKeyStore : IConfigureOptions<OpenIddictServerOptions>
{
    private readonly ISnapshotStore<State> store;

    [CollectionName("Identity_Keys")]
    public sealed class State
    {
        public string Key { get; set; }

        public RSAParameters Parameters { get; set; }
    }

    public DefaultKeyStore(ISnapshotStore<State> store)
    {
        this.store = store;
    }

    public void Configure(OpenIddictServerOptions options)
    {
        var securityKey = GetOrCreateKeyAsync().Result;

        options.SigningCredentials.Add(
            new SigningCredentials(securityKey,
                SecurityAlgorithms.RsaSha256));

        options.EncryptionCredentials.Add(new EncryptingCredentials(securityKey,
            SecurityAlgorithms.RsaOAEP,
            SecurityAlgorithms.Aes256CbcHmacSha512));
    }

    private async Task<RsaSecurityKey> GetOrCreateKeyAsync()
    {
        var (_, state, _, _) = await store.ReadAsync(default);

        RsaSecurityKey securityKey;

        var attempts = 0;

        while (state == null && attempts < 10)
        {
            securityKey = new RsaSecurityKey(RSA.Create(2048))
            {
                KeyId = CryptoRandom.CreateUniqueId(16)
            };

            state = new State { Key = securityKey.KeyId };

            if (securityKey.Rsa != null)
            {
                var parameters = securityKey.Rsa.ExportParameters(true);

                state.Parameters = parameters;
            }
            else
            {
                state.Parameters = securityKey.Parameters;
            }

            try
            {
                await store.WriteAsync(new SnapshotWriteJob<State>(default, state, 0));

                return securityKey;
            }
            catch (InconsistentStateException)
            {
                (_, state, _, _) = await store.ReadAsync(default);
            }
        }

        if (state == null)
        {
            ThrowHelper.InvalidOperationException("Cannot read key.");
            return default!;
        }

        securityKey = new RsaSecurityKey(state.Parameters)
        {
            KeyId = state.Key
        };

        return securityKey;
    }
}
