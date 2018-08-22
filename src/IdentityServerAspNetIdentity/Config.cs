using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;

namespace IdentityServerQuickStart
{
    public class Config
    {
        public static readonly string internalClient = "squidex-internal";
        public static readonly string InternalClientSecret = "squidex-internal".Sha256();
        public static readonly string OrleansPrefix = "/orleans";
        public static readonly string PortalPrefix = "/portal";
        public static readonly string ApiScope = "squidex-api";
        public static readonly string ProfileScope = "squidex-profile";
        public static readonly string RoleScope = "role";

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "My API"),
                new ApiResource(ApiScope)
                {
                    UserClaims = new List<string>
                    {
                        JwtClaimTypes.Email,
                        JwtClaimTypes.Role
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.Implicit,

                    RequireConsent = false,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris           = { "http://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1"
                    },
                    AllowOfflineAccess = true
                },
                new Client
                {
                    ClientId = "implicit",
                    ClientName = "MVC Client",
                    AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                    AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,

                    RequireConsent = false,
                    RedirectUris           = { "http://localhost:50006/identity-server/signin-oidc" },
                    

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "squidex-api",
                        "squidex-profile",
                        "role"
                    },
                    AllowOfflineAccess = true
                },
            };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password"
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password"
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource("role",
                new[]
                {
                    JwtClaimTypes.Role
                }),
                new IdentityResource("squidex-profile",
                new[]
                {
                    "urn:squidex:name",
                    "urn:squidex:picture"
                })
            };
        }
    }
}
