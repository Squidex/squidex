// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Web.Pipeline;

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.AspNetCore.Authentication;

public static class ApiKeyAuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<ApiKeyOptions, ApiKeyHandler>(ApiKeyDefaults.AuthenticationScheme, _ => { });
    }
}
