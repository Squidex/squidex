// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.IdentityServer.Controllers;

public class ExternalProvider(string authenticationSchema, string displayName)
{
    public string DisplayName { get; } = displayName;

    public string AuthenticationScheme { get; } = authenticationSchema;
}