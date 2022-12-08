// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.IdentityServer.Controllers;

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