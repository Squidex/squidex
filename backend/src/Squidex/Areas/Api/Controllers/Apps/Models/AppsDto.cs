// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class AppsDto : Resource
{
    /// <summary>
    /// The total number of apps.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// The apps.
    /// </summary>
    public AppDto[] Items { get; set; }

    /// <summary>
    /// True, if all apps are returned and not only the apps where the user is assigned to.
    /// </summary>
    public bool ContainsAllApps { get; set; }

    public static AppsDto FromDomain(IResultList<App> apps, bool containsAllApps, string userId, bool isFrontend, Resources resources)
    {
        var result = new AppsDto
        {
            Total = apps.Total,
            Items = apps.OrderBy(x => x.Name).Select(x => AppDto.FromDomain(x, userId, isFrontend, resources)).ToArray(),
            ContainsAllApps = containsAllApps,
        };

        return result.CreateLinks(resources);
    }

    private AppsDto CreateLinks(Resources resources)
    {
        AddSelfLink(resources.Url<AppsController>(c => nameof(c.GetAppsV2)));

        return this;
    }
}
