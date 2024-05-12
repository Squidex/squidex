// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.IdentityServer.Controllers.Account;

#pragma warning disable MA0048 // File name must match type name

public class LoginVM
{
    public string? ReturnUrl { get; set; }

    public string? Email { get; set; }

    public string? DynamicEmail { get; set; }

    public string? Password { get; set; }

    public bool IsLogin { get; set; }

    public bool HasPasswordAuth { get; set; }

    public bool HasCustomAuth { get; set; }

    public bool HasExternalLogin => ExternalProviders.Any();

    public RequestType RequestType { get; set; }

    public IReadOnlyList<ExternalProvider> ExternalProviders { get; set; }
}

public enum RequestType
{
    Get,
    Login,
    LoginCustom
}
