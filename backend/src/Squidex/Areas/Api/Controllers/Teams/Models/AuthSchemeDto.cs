// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Teams.Models;

[OpenApiRequest]
public sealed class AuthSchemeDto
{
    /// <summary>
    /// The domain name of your user accounts.
    /// </summary>
    [LocalizedRequired]
    public string Domain { get; init; }

    /// <summary>
    /// The display name for buttons.
    /// </summary>
    [LocalizedRequired]
    public string DisplayName { get; init; }

    /// <summary>
    /// The client ID.
    /// </summary>
    [LocalizedRequired]
    public string ClientId { get; init; }

    /// <summary>
    /// The client secret.
    /// </summary>
    [LocalizedRequired]
    public string ClientSecret { get; init; }

    /// <summary>
    /// The authority URL.
    /// </summary>
    [LocalizedRequired]
    public string Authority { get; init; }

    /// <summary>
    /// The URL to redirect after a signout.
    /// </summary>
    public string? SignoutRedirectUrl { get; init; }

    public AuthScheme ToDomain()
    {
        return SimpleMapper.Map(this, new AuthScheme());
    }

    public static AuthSchemeDto FromDomain(AuthScheme source)
    {
        return SimpleMapper.Map(source, new AuthSchemeDto());
    }
}
