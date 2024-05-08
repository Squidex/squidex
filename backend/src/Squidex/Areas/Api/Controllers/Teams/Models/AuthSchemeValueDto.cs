// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Teams.Commands;

namespace Squidex.Areas.Api.Controllers.Teams.Models;

public class AuthSchemeValueDto
{
    /// <summary>
    /// The auth scheme if configured.
    /// </summary>
    public AuthSchemeDto? Scheme { get; set; }

    public UpsertAuth ToCommand()
    {
        return new UpsertAuth
        {
            Scheme = Scheme?.ToDomain()
        };
    }

    public static AuthSchemeValueDto FromDomain(Team source)
    {
        var result = new AuthSchemeValueDto();

        if (source.AuthScheme != null)
        {
            result.Scheme = AuthSchemeDto.FromDomain(source.AuthScheme);
        }

        return result;
    }
}
