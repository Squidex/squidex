// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Security.Claims;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using ClaimsPermissions = Squidex.Infrastructure.Security.PermissionSet;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities;

public sealed class Context
{
    private static readonly IReadOnlyDictionary<string, string> EmptyHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static readonly char[] Separators = [',', ';'];

    // Splitting a header is not free and the same headers are read several times per request, for
    // example once per schema of a query. A concurrent dictionary is used because a context is
    // shared between the parallel resolvers of a GraphQL query. The context is immutable, so the
    // parsed values never have to be invalidated.
    private readonly ConcurrentDictionary<string, string[]> headerValues = new ConcurrentDictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Headers { get; }

    public ClaimsPermissions UserPermissions { get; }

    public ClaimsPrincipal UserPrincipal { get; }

    public App App { get; }

    public bool IsFrontendClient { get; }

    public Context(ClaimsPrincipal user, App app)
        : this(app, user, user.Claims.Permissions(), EmptyHeaders)
    {
        Guard.NotNull(user);
    }

    private Context(
        App app,
        ClaimsPrincipal userPrincipal,
        ClaimsPermissions userPermissions,
        IReadOnlyDictionary<string, string> headers)
    {
        App = app;

        UserPrincipal = userPrincipal;
        UserPermissions = userPermissions;

        IsFrontendClient = userPrincipal.IsInClient(DefaultClients.Frontend);

        Headers = headers;
    }

    public static Context Anonymous(App app)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return new Context(claimsPrincipal, app);
    }

    public static Context Admin(App app)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, PermissionIds.All));

        return new Context(claimsPrincipal, app);
    }

    internal string[] HeaderValues(string key)
    {
        if (headerValues.TryGetValue(key, out var result))
        {
            return result;
        }

        if (!Headers.TryGetValue(key, out var value))
        {
            return [];
        }

        result = value.Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToArray();

        headerValues[key] = result;

        return result;
    }

    public bool Allows(string permissionId, string schema = Permission.Any)
    {
        return UserPermissions.Allows(permissionId, App.Name, schema);
    }

    private sealed class HeaderBuilder(Context context) : ICloneBuilder
    {
        private Dictionary<string, string>? headers;

        public Context Build()
        {
            if (headers != null)
            {
                return new Context(context.App!, context.UserPrincipal, context.UserPermissions, headers);
            }

            return context;
        }

        public void Remove(string key)
        {
            headers ??= new Dictionary<string, string>(context.Headers, StringComparer.OrdinalIgnoreCase);
            headers.Remove(key);
        }

        public void SetHeader(string key, string value)
        {
            headers ??= new Dictionary<string, string>(context.Headers, StringComparer.OrdinalIgnoreCase);
            headers[key] = value;
        }
    }

    public Context WithApp(App app)
    {
        return new Context(app, UserPrincipal, UserPermissions, Headers);
    }

    public Context Clone(Action<ICloneBuilder> action)
    {
        var builder = new HeaderBuilder(this);

        action(builder);

        return builder.Build();
    }
}

public interface ICloneBuilder
{
    void SetHeader(string key, string value);

    void Remove(string key);
}
