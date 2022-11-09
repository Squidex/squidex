// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Entities.Apps;
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

    public IReadOnlyDictionary<string, string> Headers { get; private set; }

    public ClaimsPermissions UserPermissions { get; }

    public ClaimsPrincipal UserPrincipal { get; }

    public IAppEntity App { get; set; }

    public bool IsFrontendClient => UserPrincipal.IsInClient(DefaultClients.Frontend);

    public Context(ClaimsPrincipal user, IAppEntity app)
        : this(app, user, user.Claims.Permissions(), EmptyHeaders)
    {
        Guard.NotNull(user);
    }

    private Context(
        IAppEntity app,
        ClaimsPrincipal userPrincipal,
        ClaimsPermissions userPermissions,
        IReadOnlyDictionary<string, string> headers)
    {
        App = app;

        UserPrincipal = userPrincipal;
        UserPermissions = userPermissions;

        Headers = headers;
    }

    public static Context Anonymous(IAppEntity app)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return new Context(claimsPrincipal, app);
    }

    public static Context Admin(IAppEntity app)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, PermissionIds.All));

        return new Context(claimsPrincipal, app);
    }

    public bool Allows(string permissionId, string schema = Permission.Any)
    {
        return UserPermissions.Allows(permissionId, App.Name, schema);
    }

    private sealed class HeaderBuilder : ICloneBuilder
    {
        private readonly Context context;
        private Dictionary<string, string>? headers;

        public HeaderBuilder(Context context)
        {
            this.context = context;
        }

        public Context Build()
        {
            if (headers != null)
            {
                return new Context(context.App!, context.UserPrincipal, context.UserPermissions, headers);
            }

            return context;
        }

        public Context Update()
        {
            context.Headers = headers ?? context.Headers;

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

    public Context Change(Action<ICloneBuilder> action)
    {
        var builder = new HeaderBuilder(this);

        action(builder);

        return builder.Update();
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
