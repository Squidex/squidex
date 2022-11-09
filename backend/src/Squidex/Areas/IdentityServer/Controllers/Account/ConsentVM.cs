// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.IdentityServer.Controllers.Account;

public sealed class ConsentVM
{
    public string? ReturnUrl { get; set; }

    public string? PrivacyUrl { get; set; }
}
