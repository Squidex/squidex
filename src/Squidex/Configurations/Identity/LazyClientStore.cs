// ==========================================================================
//  LazyClientStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;

namespace Squidex.Configurations.Identity
{
    public class LazyClientStore : IClientStore
    {
        private readonly Dictionary<string, Client> clients = new Dictionary<string, Client>(StringComparer.OrdinalIgnoreCase);

        public LazyClientStore(IOptions<MyIdentityOptions> identityOptions)
        {
            Guard.NotNull(identityOptions, nameof(identityOptions));

            foreach (var client in CreateClients(identityOptions.Value))
            {
                clients[client.ClientId] = client;
            }
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var client = clients.GetOrDefault(clientId);

            return Task.FromResult(client);
        }

        private static IEnumerable<Client> CreateClients(MyIdentityOptions options)
        {
            const string id = Constants.FrontendClient;

            yield return new Client
            {
                ClientId = id,
                ClientName = id,
                RedirectUris = new List<string>
                {
                    options.BuildUrl("login;"),
                    options.BuildUrl("identity-server/client-callback-silent/"),
                    options.BuildUrl("identity-server/client-callback-popup/")
                },
                PostLogoutRedirectUris = new List<string>
                {
                    options.BuildUrl("logout", false)
                },
                AllowAccessTokensViaBrowser = true,
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = new List<string>
                {
                    StandardScopes.OpenId.Name,
                    StandardScopes.Profile.Name,
                    Constants.ApiScope,
                    Constants.ProfileScope
                },
                RequireConsent = false
            };
        }
    }
}
