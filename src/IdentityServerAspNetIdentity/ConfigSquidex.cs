using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;

namespace IdentityServerQuickStart
{
    public class ConfigSquidex
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
                new ApiResource(ApiScope)
                {
                    UserClaims = new List<string>
                    {
                        //JwtClaimTypes.Name,
                        //JwtClaimTypes.Subject,
                        JwtClaimTypes.Email,
                        JwtClaimTypes.Role,
                        //JwtClaimTypes.SessionId                        
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
                    ClientId = "my-client",
                    ClientName = "my-client",
                    ClientSecrets = new List<Secret> { new Secret(InternalClientSecret) },                    
                    RedirectUris = new List<string>
                    {
                        "http://localhost:50006/portal/signin-oidc",
                        "http://localhost:50006/orleans/signin-oidc",
                        "http://localhost:50006/identity-server/signin-oidc"
                    },
                    AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                    AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
                    AllowAccessTokensViaBrowser = true,
                    AllowedCorsOrigins =     { "http://localhost:50006" },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        ApiScope,
                        ProfileScope,
                        RoleScope,
                        "access_token"
                    },
                    RequireConsent = false,
                    AlwaysSendClientClaims = true
                },
                new Client
                {
                    ClientId = "squidex-frontend",
                    ClientName = "squidex-frontend",
                    RedirectUris = new List<string>
                    {
                        "http://localhost:50006/login;",
                        "http://localhost:50006/client-callback-silent",
                        "http://localhost:50006/client-callback-popup"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:50006/logout"
                    },
                    AllowAccessTokensViaBrowser = true,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        ApiScope,
                        ProfileScope,
                        RoleScope
                    },
                    RequireConsent = false
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
