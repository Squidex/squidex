// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Shared.Identity
{
    public static class SquidexClaimTypes
    {
        public static readonly string ClientSecret = "urn:squidex:clientSecret";

        public static readonly string Consent = "urn:squidex:consent";

        public static readonly string ConsentForEmails = "urn:squidex:consent:emails";

        public static readonly string CustomPrefix = "urn:squidex:custom";

        public static readonly string DisplayName = "urn:squidex:name";

        public static readonly string Hidden = "urn:squidex:hidden";

        public static readonly string Invited = "urn:squidex:invited";

        public static readonly string NotifoKey = "urn:squidex:notifo";

        public static readonly string Permissions = "urn:squidex:permissions";

        public static readonly string PictureUrl = "urn:squidex:picture";

        public static readonly string PictureUrlStore = "store";
    }
}
