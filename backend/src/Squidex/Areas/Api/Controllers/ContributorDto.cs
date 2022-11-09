// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Apps;
using Squidex.Areas.Api.Controllers.Teams;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers;

public sealed class ContributorDto : Resource
{
    /// <summary>
    /// The ID of the user that contributes to the app.
    /// </summary>
    [LocalizedRequired]
    public string ContributorId { get; set; }

    /// <summary>
    /// The display name.
    /// </summary>
    [LocalizedRequired]
    public string ContributorName { get; set; }

    /// <summary>
    /// The email address.
    /// </summary>
    [LocalizedRequired]
    public string ContributorEmail { get; set; }

    /// <summary>
    /// The role of the contributor.
    /// </summary>
    public string? Role { get; set; }

    public static ContributorDto FromDomain(string id, string role)
    {
        var result = new ContributorDto { ContributorId = id, Role = role };

        return result;
    }

    public ContributorDto CreateUser(IDictionary<string, IUser> users)
    {
        if (users.TryGetValue(ContributorId, out var user))
        {
            ContributorName = user.Claims.DisplayName()!;
            ContributorEmail = user.Email;
        }
        else
        {
            ContributorName = T.Get("common.notFoundValue");
        }

        return this;
    }

    public ContributorDto CreateAppLinks(Resources resources)
    {
        if (resources.IsUser(ContributorId))
        {
            return this;
        }

        var app = resources.App;

        if (resources.CanAssignContributor)
        {
            var values = new { app };

            AddPostLink("update",
                resources.Url<AppContributorsController>(x => nameof(x.PostContributor), values));
        }

        if (resources.CanRevokeContributor)
        {
            var values = new { app, id = ContributorId };

            AddDeleteLink("delete",
                resources.Url<AppContributorsController>(x => nameof(x.DeleteContributor), values));
        }

        return this;
    }

    public ContributorDto CreateTeamLinks(Resources resources)
    {
        if (resources.IsUser(ContributorId))
        {
            return this;
        }

        var team = resources.Team;

        if (resources.CanAssignTeamContributor)
        {
            var values = new { team };

            AddPostLink("update",
                resources.Url<TeamContributorsController>(x => nameof(x.PostContributor), values));
        }

        if (resources.CanRevokeTeamContributor)
        {
            var values = new { team, id = ContributorId };

            AddDeleteLink("delete",
                resources.Url<TeamContributorsController>(x => nameof(x.DeleteContributor), values));
        }

        return this;
    }
}
