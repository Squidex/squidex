// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using IdentityServer4.Models;

namespace Squidex.Config
{
    public static class Constants
    {
        public static readonly string SecurityDefinition = "squidex-oauth-auth";

        public static readonly string ApiPrefix = "/api";

        public static readonly string ApiScope = "squidex-api";

        public static readonly string PortalPrefix = "/portal";

        public static readonly string RoleScope = "role";

        public static readonly string ProfileScope = "squidex-profile";

        public static readonly string FrontendClient = "squidex-frontend";

        public static readonly string InternalClientId = "squidex-internal";

        public static readonly string InternalClientSecret = "squidex-internal".Sha256();

        public static readonly string IdentityServerPrefix = "/identity-server";
    }
}
