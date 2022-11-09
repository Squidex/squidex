// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Squidex.Config;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Users;
using Squidex.Domain.Users.InMemory;
using Squidex.Hosting;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Squidex.Web;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Squidex.Areas.IdentityServer.Config;

public class DynamicApplicationStore : InMemoryApplicationStore
{
    private readonly IServiceProvider serviceProvider;

    public DynamicApplicationStore(IServiceProvider serviceProvider)
        : base(CreateStaticClients(serviceProvider))
    {
        this.serviceProvider = serviceProvider;
    }

    public override async ValueTask<ImmutableApplication?> FindByIdAsync(string identifier,
        CancellationToken cancellationToken)
    {
        var application = await base.FindByIdAsync(identifier, cancellationToken);

        if (application == null)
        {
            application = await GetDynamicAsync(identifier);
        }

        return application;
    }

    public override async ValueTask<ImmutableApplication?> FindByClientIdAsync(string identifier,
        CancellationToken cancellationToken)
    {
        var application = await base.FindByClientIdAsync(identifier, cancellationToken);

        if (application == null)
        {
            application = await GetDynamicAsync(identifier);
        }

        return application;
    }

    private async Task<ImmutableApplication?> GetDynamicAsync(string clientId)
    {
        var (appName, appClientId) = clientId.GetClientParts();

        var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

        if (!string.IsNullOrWhiteSpace(appName) && !string.IsNullOrWhiteSpace(appClientId))
        {
            var app = await appProvider.GetAppAsync(appName, true);

            var appClient = app?.Clients.GetValueOrDefault(appClientId);

            if (appClient != null)
            {
                return CreateClientFromApp(clientId, appClient);
            }
        }

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var user = await userService.FindByIdAsync(clientId);

            if (user == null)
            {
                return null;
            }

            var secret = user.Claims.ClientSecret();

            if (!string.IsNullOrWhiteSpace(secret))
            {
                return CreateClientFromUser(user, secret);
            }
        }

        return null;
    }

    private static ImmutableApplication CreateClientFromUser(IUser user, string secret)
    {
        return new ImmutableApplication(user.Id, new OpenIddictApplicationDescriptor
        {
            DisplayName = $"{user.Email} Client",
            ClientId = user.Id,
            ClientSecret = secret,
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.ResponseTypes.Token,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + Constants.ScopeApi,
                Permissions.Prefixes.Scope + Constants.ScopePermissions
            }
        }.CopyClaims(user));
    }

    private static ImmutableApplication CreateClientFromApp(string id, AppClient appClient)
    {
        return new ImmutableApplication(id, new OpenIddictApplicationDescriptor
        {
            DisplayName = id,
            ClientId = id,
            ClientSecret = appClient.Secret,
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.ResponseTypes.Token,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + Constants.ScopeApi,
                Permissions.Prefixes.Scope + Constants.ScopePermissions
            }
        });
    }

    private static IEnumerable<(string, OpenIddictApplicationDescriptor)> CreateStaticClients(IServiceProvider serviceProvider)
    {
        var identityOptions = serviceProvider.GetRequiredService<IOptions<MyIdentityOptions>>().Value;

        var urlGenerator = serviceProvider.GetRequiredService<IUrlGenerator>();

        var frontendId = Constants.ClientFrontendId;

        yield return (frontendId, new OpenIddictApplicationDescriptor
        {
            DisplayName = "Frontend Client",
            ClientId = frontendId,
            ClientSecret = null,
            RedirectUris =
            {
                new Uri(urlGenerator.BuildUrl("login;")),
                new Uri(urlGenerator.BuildUrl("client-callback-silent.html", false)),
                new Uri(urlGenerator.BuildUrl("client-callback-popup.html", false))
            },
            PostLogoutRedirectUris =
            {
                new Uri(urlGenerator.BuildUrl("logout", false))
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Logout,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + Constants.ScopeApi,
                Permissions.Prefixes.Scope + Constants.ScopePermissions
            },
            Type = ClientTypes.Public
        });

        var internalClientId = Constants.ClientInternalId;

        yield return (internalClientId, new OpenIddictApplicationDescriptor
        {
            DisplayName = "Internal Client",
            ClientId = internalClientId,
            ClientSecret = Constants.ClientInternalSecret,
            RedirectUris =
            {
                new Uri(urlGenerator.BuildUrl("/signin-internal", false))
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Logout,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.Implicit,
                Permissions.ResponseTypes.IdToken,
                Permissions.ResponseTypes.IdTokenToken,
                Permissions.ResponseTypes.Token,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + Constants.ScopeApi,
                Permissions.Prefixes.Scope + Constants.ScopePermissions
            },
            Type = ClientTypes.Public
        });

        if (!identityOptions.IsAdminClientConfigured())
        {
            yield break;
        }

        var adminClientId = identityOptions.AdminClientId;

        yield return (adminClientId, new OpenIddictApplicationDescriptor
        {
            DisplayName = "Admin Client",
            ClientId = adminClientId,
            ClientSecret = identityOptions.AdminClientSecret,
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.ResponseTypes.Token,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + Constants.ScopeApi,
                Permissions.Prefixes.Scope + Constants.ScopePermissions
            }
        }.SetAdmin());
    }
}
