// ==========================================================================
//  ExternalLogin.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Read.Users
{
    public sealed class ExternalLogin
    {
        public string LoginProvider { get; }

        public string ProviderKey { get; }

        public string ProviderDisplayName { get; }

        public ExternalLogin(string loginProvider, string providerKey, string providerDisplayName)
        {
            LoginProvider = loginProvider;

            ProviderKey = providerKey;
            ProviderDisplayName = providerDisplayName;

            if (string.IsNullOrWhiteSpace(ProviderDisplayName))
            {
                ProviderDisplayName = loginProvider;
            }
        }
    }
}
