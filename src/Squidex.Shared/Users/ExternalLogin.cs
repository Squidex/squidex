// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Shared.Users
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
