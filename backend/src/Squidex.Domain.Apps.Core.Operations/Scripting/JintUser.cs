// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Jint;
using Jint.Native;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Scripting;

public static class JintUser
{
    private static readonly char[] ClaimSeparators = { '/', '.', ':' };

    public static JsValue Create(Engine engine, IUser user)
    {
        var isClient = user.Claims.Any(x => x.Type == OpenIdClaims.ClientId);

        return CreateUser(
            engine,
            user.Id,
            isClient,
            user.Email,
            user.Claims.DisplayName(),
            user.Claims);
    }

    public static JsValue Create(Engine engine, ClaimsPrincipal principal)
    {
        var token = principal.Token();

        return CreateUser(
            engine,
            token?.Identifier ?? string.Empty,
            token?.Type != RefTokenType.Subject,
            principal.OpenIdEmail()!,
            principal.Claims.DisplayName(),
            principal.Claims);
    }

    private static JsValue CreateUser(Engine engine, string id, bool isClient, string email, string? name, IEnumerable<Claim> allClaims)
    {
        var claims =
            allClaims.GroupBy(x => x.Type.Split(ClaimSeparators)[^1])
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(y => y.Value).ToArray());

        var result = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["email"] = email,
            ["isClient"] = isClient,
            ["isUser"] = !isClient,
            ["name"] = name,
            ["claims"] = claims
        };

        return JsValue.FromObject(engine, result);
    }
}
