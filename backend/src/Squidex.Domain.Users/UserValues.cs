// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Users;

public sealed class UserValues
{
    public string? DisplayName { get; set; }

    public string? PictureUrl { get; set; }

    public string? Password { get; set; }

    public string? ClientSecret { get; set; }

    public string Email { get; set; }

    public bool? Hidden { get; set; }

    public bool? Invited { get; set; }

    public bool? Consent { get; set; }

    public bool? ConsentForEmails { get; set; }

    public PermissionSet? Permissions { get; set; }

    public List<Claim>? CustomClaims { get; set; }

    public List<(string Name, string Value)>? Properties { get; set; }
}
