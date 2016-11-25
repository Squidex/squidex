// ==========================================================================
//  ExternalProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Controllers.UI.Account
{
    public class ExternalProvider
    {
        public string DisplayName { get; }

        public string AuthenticationScheme { get; }

        public ExternalProvider(string authenticationSchema, string displayName)
        {
            AuthenticationScheme = authenticationSchema;

            DisplayName = displayName;
        }
    }
}