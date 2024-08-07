﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;

namespace Squidex.Areas.IdentityServer.Controllers.Profile;

public sealed class ProfileVM
{
    public string Id { get; set; }

    public string Email { get; set; }

    public string DisplayName { get; set; }

    public string? CompanyRole { get; set; }

    public string? CompanySize { get; set; }

    public string? Project { get; set; }

    public string? OldPassword { get; set; }

    public string? Password { get; set; }

    public string? PasswordConfirm { get; set; }

    public string? ClientSecret { get; set; }

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public bool ShowAbout { get; set; }

    public bool IsHidden { get; set; }

    public bool HasPassword { get; set; }

    public bool HasPasswordAuth { get; set; }

    public List<UserProperty> Properties { get; set; }

    public IList<UserLoginInfo> ExternalLogins { get; set; }

    public IList<ExternalProvider> ExternalProviders { get; set; }
}
