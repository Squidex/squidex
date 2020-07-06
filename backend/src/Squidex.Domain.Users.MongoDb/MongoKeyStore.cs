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
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoKeyStore : MongoRepositoryBase<MongoKey>, ISigningCredentialStore, IValidationKeysStore
    {
        private SigningCredentials? cachedKey;
        private SecurityKeyInfo[]? cachedKeyInfo;

        public MongoKeyStore(IMongoDatabase database, bool setup = false)
            : base(database, setup)
        {
        }

        protected override string CollectionName()
        {
            return "Key";
        }

        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            var (_, key) = await GetOrCreateKeyAsync();
            // SignatureProvider signatureProvider = key.CryptoProviderFactory.CreateForVerifying(key, key.Al);

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

            var key = await Collection.Find(x => x.Id == "Default").FirstOrDefaultAsync();

            RsaSecurityKey securityKey;

            if (key == null)
            {
                securityKey = new RsaSecurityKey(RSA.Create(2048))
                {
                    KeyId = CryptoRandom.CreateUniqueId(16)
                };

                key = new MongoKey { Id = "Default", Key = securityKey.KeyId };

                if (securityKey.Rsa != null)
                {
                    var parameters = securityKey.Rsa.ExportParameters(includePrivateParameters: true);

                    key.Parameters = MongoKeyParameters.Create(parameters);
                }
                else
                {
                    key.Parameters = MongoKeyParameters.Create(securityKey.Parameters);
                }

                try
                {
                    await Collection.InsertOneAsync(key);

                    return CreateCredentialsPair(securityKey);
                }
                catch (MongoWriteException ex)
                {
                    if (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                    {
                        key = await Collection.Find(x => x.Id == "Default").FirstOrDefaultAsync();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (key == null)
            {
                throw new InvalidOperationException("Cannot read key.");
            }

            securityKey = new RsaSecurityKey(key.Parameters.ToParameters())
            {
                KeyId = key.Key
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
