// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Shared;

namespace Squidex.Web;

public static class Constants
{
    public const string SecurityDefinition = "squidex-oauth-auth";

    public const string ApiSecurityScheme = "API";

    public const string PrefixApi = "/api";

    public const string PrefixIdentityServer = "/identity-server";

    public const string ScopePermissions = "permissions";

    public const string ScopeProfile = "squidex-profile";

    public const string ScopeRole = "role";

    public const string ScopeApi = "squidex-api";

    public static readonly string ClientFrontendId = DefaultClients.Frontend;

    public static readonly string ClientInternalId = "squidex-internal";

    public static readonly string ClientInternalSecret = "squidex-internal".ToSha256Base64();
}
