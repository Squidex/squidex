// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Areas.Api.Controllers.Apps;
using Squidex.Areas.Api.Controllers.Teams;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers;

public sealed class ContributorsDto : Resource
{
    /// <summary>
    /// The contributors.
    /// </summary>
    [LocalizedRequired]
    public ContributorDto[] Items { get; set; }

    /// <summary>
    /// The maximum number of allowed contributors.
    /// </summary>
    public long MaxContributors { get; set; }

    /// <summary>
    /// The metadata to provide information about this request.
    /// </summary>
    [JsonPropertyName("_meta")]
    public ContributorsMetadata? Metadata { get; set; }

    public static async Task<ContributorsDto> FromDomainAsync(IAppEntity app, Resources resources, IUserResolver userResolver, Plan plan, bool invited)
    {
        var users = await userResolver.QueryManyAsync(app.Contributors.Keys.ToArray());

        var result = new ContributorsDto
        {
            Items = app.Contributors
                .Select(x => ContributorDto.FromDomain(x.Key, x.Value))
                .Select(x => x.CreateUser(users))
                .Select(x => x.CreateAppLinks(resources))
                .OrderBy(x => x.ContributorName)
                .ToArray()
        };

        result.CreateInvited(invited);
        result.CreatePlan(plan);

        return result.CreateAppLinks(resources);
    }

    public static async Task<ContributorsDto> FromDomainAsync(ITeamEntity team, Resources resources, IUserResolver userResolver, bool invited)
    {
        var users = await userResolver.QueryManyAsync(team.Contributors.Keys.ToArray());

        var result = new ContributorsDto
        {
            Items = team.Contributors
                .Select(x => ContributorDto.FromDomain(x.Key, x.Value))
                .Select(x => x.CreateUser(users))
                .Select(x => x.CreateTeamLinks(resources))
                .OrderBy(x => x.ContributorName)
                .ToArray()
        };

        result.CreateInvited(invited);

        return result.CreateTeamLinks(resources);
    }

    private void CreatePlan(Plan plan)
    {
        MaxContributors = plan.MaxContributors;
    }

    private void CreateInvited(bool isInvited)
    {
        if (isInvited)
        {
            Metadata = new ContributorsMetadata
            {
                IsInvited = "true"
            };
        }
    }

    private ContributorsDto CreateAppLinks(Resources resources)
    {
        var values = new { app = resources.App };

        AddSelfLink(resources.Url<AppContributorsController>(x => nameof(x.GetContributors), values));

        if (resources.CanAssignContributor && (MaxContributors < 0 || Items.Length < MaxContributors))
        {
            AddPostLink("create",
                resources.Url<AppContributorsController>(x => nameof(x.PostContributor), values));
        }

        return this;
    }

    private ContributorsDto CreateTeamLinks(Resources resources)
    {
        var values = new { team = resources.Team };

        AddSelfLink(resources.Url<TeamContributorsController>(x => nameof(x.GetContributors), values));

        if (resources.CanAssignTeamContributor)
        {
            AddPostLink("create",
                resources.Url<TeamContributorsController>(x => nameof(x.PostContributor), values));
        }

        return this;
    }
}
