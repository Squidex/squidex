// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Teams.Models;

public class AuthSchemeResponseDto : Resource
{
    /// <summary>
    /// The auth scheme if configured.
    /// </summary>
    public AuthSchemeDto? Scheme { get; set; }

    /// <summary>
    /// True, if the ownership of the domain has been verified.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// The name of the DNS TXT record to verify the ownership of the domain.
    /// </summary>
    public string? VerificationRecordName { get; set; }

    /// <summary>
    /// The value of the DNS TXT record to verify the ownership of the domain.
    /// </summary>
    public string? VerificationRecordValue { get; set; }

    public static AuthSchemeResponseDto FromDomain(Team team, Resources resources)
    {
        var result = new AuthSchemeResponseDto();

        if (team.AuthScheme != null)
        {
            result.Scheme = AuthSchemeDto.FromDomain(team.AuthScheme);
            result.IsVerified = team.IsAuthDomainVerified;
            result.VerificationRecordName = AuthDomainRecord.GetName(team.AuthScheme.Domain);
            result.VerificationRecordValue = AuthDomainRecord.GetValue(team.Id);
        }

        return result.CreateLinks(resources);
    }

    private AuthSchemeResponseDto CreateLinks(Resources resources)
    {
        var values = new { team = resources.Team };

        AddSelfLink(resources.Url<TeamsController>(x => nameof(x.GetTeamAuth), values));

        if (!resources.CanChangeTeamAuth)
        {
            return this;
        }

        AddPutLink("update",
            resources.Url<TeamsController>(x => nameof(x.PutTeamAuth), values));

        if (Scheme != null && !IsVerified)
        {
            AddPostLink("verify",
                resources.Url<TeamsController>(x => nameof(x.PostTeamAuthVerify), values));
        }

        return this;
    }
}
