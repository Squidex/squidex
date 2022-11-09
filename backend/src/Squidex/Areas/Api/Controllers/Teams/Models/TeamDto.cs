// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Areas.Api.Controllers.Plans;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Teams.Models;

public sealed class TeamDto : Resource
{
    /// <summary>
    /// The ID of the team.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The name of the team.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    /// <summary>
    /// The version of the team.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// The timestamp when the team has been created.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The timestamp when the team has been modified last.
    /// </summary>
    public Instant LastModified { get; set; }

    /// <summary>
    /// The role name of the user.
    /// </summary>
    public string? RoleName { get; set; }

    public static TeamDto FromDomain(ITeamEntity team, string userId, Resources resources)
    {
        var result = SimpleMapper.Map(team, new TeamDto());

        var permissions = PermissionSet.Empty;

        if (team.TryGetContributorRole(userId, out _))
        {
            permissions = new PermissionSet(PermissionIds.ForApp(PermissionIds.TeamAdmin, team: team.Id.ToString()));
        }

        return result.CreateLinks(team, resources, permissions, true);
    }

    private TeamDto CreateLinks(ITeamEntity team, Resources resources, PermissionSet permissions, bool isContributor)
    {
        var values = new { team = Id.ToString() };

        if (isContributor)
        {
            AddDeleteLink("leave",
                resources.Url<TeamContributorsController>(x => nameof(x.DeleteMyself), values));
        }

        if (resources.IsAllowed(PermissionIds.TeamUpdate, team: values.team, additional: permissions))
        {
            AddPutLink("update",
                resources.Url<TeamsController>(x => nameof(x.PutTeam), values));
        }

        if (resources.IsAllowed(PermissionIds.TeamContributorsRead, team: values.team, additional: permissions))
        {
            AddGetLink("contributors",
                resources.Url<TeamContributorsController>(x => nameof(x.GetContributors), values));
        }

        if (resources.IsAllowed(PermissionIds.TeamPlansRead, team: values.team, additional: permissions))
        {
            AddGetLink("plans",
                resources.Url<TeamPlansController>(x => nameof(x.GetTeamPlans), values));
        }

        return this;
    }
}
