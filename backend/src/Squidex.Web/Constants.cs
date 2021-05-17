// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Shared;

namespace Squidex.Web
{
    public static class Constants
    {
        public static readonly string SecurityDefinition = "squidex-oauth-auth";

        public static readonly string OrleansClusterId = "squidex-v2";

        public static readonly string ApiSecurityScheme = "API";

        public static readonly string PrefixApi = "/api";

        public static readonly string PrefixOrleans = "/orleans";

        public static readonly string PrefixPortal = "/portal";

        public static readonly string PrefixIdentityServer = "/identity-server";

        public static readonly string ScopePermissions = "permissions";

        public static readonly string ScopeProfile = "squidex-profile";

        public static readonly string ScopeRole = "role";

        public static readonly string ScopeApi = "squidex-api";

        public static readonly string ClientFrontendId = DefaultClients.Frontend;

        public static readonly string ClientInternalId = "squidex-internal";

        public static readonly string ClientInternalSecret = "squidex-internal".ToSha256Base64();
    }
}
