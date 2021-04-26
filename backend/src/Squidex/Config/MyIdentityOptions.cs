// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Config
{
    public sealed class MyIdentityOptions
    {
        public string PrivacyUrl { get; set; }

        public string AuthorityUrl { get; set; }

        public string AdminEmail { get; set; }

        public string AdminPassword { get; set; }

        public string[] AdminApps { get; set; }

        public string AdminClientId { get; set; }

        public string AdminClientSecret { get; set; }

        public string GithubClient { get; set; }

        public string GithubSecret { get; set; }

        public string GoogleClient { get; set; }

        public string GoogleSecret { get; set; }

        public string MicrosoftClient { get; set; }

        public string MicrosoftSecret { get; set; }

        public string MicrosoftTenant { get; set; }

        public string OidcName { get; set; }

        public string OidcClient { get; set; }

        public string OidcSecret { get; set; }

        public string OidcAuthority { get; set; }

        public string OidcMetadataAddress { get; set; }

        public string OidcRoleClaimType { get; set; }

        public string OidcResponseType { get; set; }

        public string OidcOnSignoutRedirectUrl { get; set; }

        public string[] OidcScopes { get; set; }

        public bool OidcGetClaimsFromUserInfoEndpoint { get; set; }

        public Dictionary<string, string[]> OidcRoleMapping { get; set; }

        public bool AdminRecreate { get; set; }

        public bool AllowPasswordAuth { get; set; }

        public bool LockAutomatically { get; set; }

        public bool NoConsent { get; set; }

        public bool RequiresHttps { get; set; }

        public bool ShowPII { get; set; }

        public bool IsAdminConfigured()
        {
            return !string.IsNullOrWhiteSpace(AdminEmail) && !string.IsNullOrWhiteSpace(AdminPassword);
        }

        public bool IsAdminClientConfigured()
        {
            return !string.IsNullOrWhiteSpace(AdminClientId) && !string.IsNullOrWhiteSpace(AdminClientSecret);
        }

        public bool IsOidcConfigured()
        {
            return !string.IsNullOrWhiteSpace(OidcAuthority) && !string.IsNullOrWhiteSpace(OidcClient);
        }

        public bool IsGithubAuthConfigured()
        {
            return !string.IsNullOrWhiteSpace(GithubClient) && !string.IsNullOrWhiteSpace(GithubSecret);
        }

        public bool IsGoogleAuthConfigured()
        {
            return !string.IsNullOrWhiteSpace(GoogleClient) && !string.IsNullOrWhiteSpace(GoogleSecret);
        }

        public bool IsMicrosoftAuthConfigured()
        {
            return !string.IsNullOrWhiteSpace(MicrosoftClient) && !string.IsNullOrWhiteSpace(MicrosoftSecret);
        }
    }
}
