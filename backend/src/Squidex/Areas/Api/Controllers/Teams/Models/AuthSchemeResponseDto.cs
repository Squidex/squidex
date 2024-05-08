// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Teams.Models;

public class AuthSchemeResponseDto : Resource
{
    /// <summary>
    /// The auth scheme if configured.
    /// </summary>
    public AuthSchemeDto? Scheme { get; set; }

    public static AuthSchemeResponseDto FromDomain(Team team, Resources resources)
    {
        var result = new AuthSchemeResponseDto();

        if (team.AuthScheme != null)
        {
            result.Scheme = AuthSchemeDto.FromDomain(team.AuthScheme);
        }

        return result.CreateLinks(resources);
    }

    private AuthSchemeResponseDto CreateLinks(Resources resources)
    {
        var values = new { team = resources.Team };

        AddSelfLink(resources.Url<TeamsController>(x => nameof(x.GetTeamAuth), values));

        if (resources.CanChangeTeamAuth)
        {
            AddPutLink("update",
                resources.Url<TeamsController>(x => nameof(x.PutTeamAuth), values));
        }

        return this;
    }
}
