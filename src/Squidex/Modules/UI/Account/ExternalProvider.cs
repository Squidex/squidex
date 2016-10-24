// ==========================================================================
//  ExternalProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Modules.UI.Account
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