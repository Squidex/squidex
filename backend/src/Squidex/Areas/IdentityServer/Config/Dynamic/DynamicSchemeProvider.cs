// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Config.Authentication;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Areas.IdentityServer.Config;

public sealed class DynamicSchemeProvider : AuthenticationSchemeProvider, IOptionsMonitor<DynamicOpenIdConnectOptions>
{
    private static readonly string[] UrlPrefixes = ["signin-", "signout-callback-", "signout-"];

    private readonly IAppProvider appProvider;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IDistributedCache dynamicCache;
    private readonly IJsonSerializer jsonSerializer;
    private readonly OpenIdConnectPostConfigureOptions configure;

    public DynamicOpenIdConnectOptions CurrentValue => null!;

    private sealed record SchemeResult(AuthenticationScheme Scheme, DynamicOpenIdConnectOptions Options);

    public DynamicSchemeProvider(
        IAppProvider appProvider,
        IHttpContextAccessor httpContextAccessor,
        IDistributedCache dynamicCache,
        IJsonSerializer jsonSerializer,
        OpenIdConnectPostConfigureOptions configure,
        IOptions<AuthenticationOptions> options)
        : base(options)
    {
        this.appProvider = appProvider;
        this.httpContextAccessor = httpContextAccessor;
        this.dynamicCache = dynamicCache;
        this.jsonSerializer = jsonSerializer;
        this.configure = configure;
    }

    public async Task<string> AddTemporarySchemeAsync(AuthScheme scheme,
        CancellationToken ct = default)
    {
        var id = Guid.NewGuid().ToString();

        var serialized = jsonSerializer.SerializeToBytes(scheme);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        await dynamicCache.SetAsync(CacheKey(id), serialized, options, ct);
        return id;
    }

    public async Task<AuthenticationScheme?> GetSchemaByEmailAddressAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var parts = email.Split('@');

        if (parts.Length != 2)
        {
            return null;
        }

        var team = await appProvider.GetTeamByAuthDomainAsync(parts[1], default);

        if (team?.AuthScheme != null)
        {
            return CreateScheme(team.Id.ToString(), team.AuthScheme).Scheme;
        }

        return null;
    }

    public override async Task<AuthenticationScheme?> GetSchemeAsync(string name)
    {
        var result = await GetSchemeCoreAsync(name, default);

        if (result != null)
        {
            return result.Scheme;
        }

        return await base.GetSchemeAsync(name);
    }

    public override async Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
    {
        var result = (await base.GetRequestHandlerSchemesAsync()).ToList();

        if (httpContextAccessor.HttpContext == null)
        {
            return result;
        }

        var path = httpContextAccessor.HttpContext.Request.Path.Value;

        if (string.IsNullOrWhiteSpace(path))
        {
            return result;
        }

        var lastSegment = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;

        foreach (var prefix in UrlPrefixes)
        {
            if (lastSegment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var name = lastSegment[prefix.Length..];

                var scheme = await GetSchemeCoreAsync(name, httpContextAccessor.HttpContext.RequestAborted);

                if (scheme != null)
                {
                    result.Add(scheme.Scheme);
                }
            }
        }

        return result;
    }

    public DynamicOpenIdConnectOptions Get(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new DynamicOpenIdConnectOptions();
        }

        var scheme = GetSchemeCoreAsync(name, default).Result;

        return scheme?.Options ?? new DynamicOpenIdConnectOptions();
    }

    private async Task<SchemeResult?> GetSchemeCoreAsync(string name,
        CancellationToken ct)
    {
        if (!Guid.TryParse(name, out _))
        {
            return null;
        }

        var cacheKey = ("DYNAMIC_SCHEME", name);

        if (httpContextAccessor.HttpContext?.Items.TryGetValue(cacheKey, out var cached) == true)
        {
            return cached as SchemeResult;
        }

        var scheme =
            await GetSchemeByTeamAsync(name, ct) ??
            await GetSchemeByTempNameAsync(name, ct);

        var result =
            scheme != null ?
            CreateScheme(name, scheme) :
            null;

        if (httpContextAccessor.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Items[cacheKey] = result;
        }

        return result;
    }

    private async Task<AuthScheme?> GetSchemeByTeamAsync(string name,
        CancellationToken ct)
    {
        var app = await appProvider.GetTeamAsync(DomainId.Create(name), ct);

        return app?.AuthScheme;
    }

    private async Task<AuthScheme?> GetSchemeByTempNameAsync(string name,
        CancellationToken ct)
    {
        var value = await dynamicCache.GetAsync(CacheKey(name), ct);

        return value != null ? jsonSerializer.Deserialize<AuthScheme>(new MemoryStream(value)) : null;
    }

    private SchemeResult CreateScheme(string name, AuthScheme config)
    {
        var scheme = new AuthenticationScheme(name, config.DisplayName, typeof(DynamicOpenIdConnectHandler));

        var options = new DynamicOpenIdConnectOptions
        {
            Events = new OidcHandler(new MyIdentityOptions
            {
                OidcOnSignoutRedirectUrl = config.SignoutRedirectUrl
            }),
            Authority = config.Authority,
            CallbackPath = new PathString($"/signin-{name}"),
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret,
            RemoteSignOutPath = new PathString($"/signout-{name}"),
            RequireHttpsMetadata = false,
            ResponseType = "code",
            SignedOutRedirectUri = new PathString($"/signout-callback-{name}")
        };

        configure.PostConfigure(name, options);

        return new SchemeResult(scheme, options);
    }

    public IDisposable? OnChange(Action<DynamicOpenIdConnectOptions, string?> listener)
    {
        return null;
    }

    private static string CacheKey(string id)
    {
        return $"AUTH_SCHEMES_{id}";
    }
}
