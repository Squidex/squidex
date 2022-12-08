// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Backups;
using Squidex.Areas.Api.Controllers.EventConsumers;
using Squidex.Areas.Api.Controllers.Languages;
using Squidex.Areas.Api.Controllers.Ping;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users.Models;

public sealed class ResourcesDto : Resource
{
    public static ResourcesDto FromDomain(Resources resources)
    {
        var result = new ResourcesDto();

        result.AddGetLink("ping",
            resources.Url<PingController>(x => nameof(x.GetPing)));

        if (resources.CanReadEvents)
        {
            result.AddGetLink("admin/events",
                resources.Url<EventConsumersController>(x => nameof(x.GetEventConsumers)));
        }

        if (resources.CanRestoreBackup)
        {
            result.AddGetLink("admin/restore",
                resources.Url<RestoreController>(x => nameof(x.GetRestoreJob)));
        }

        if (resources.CanReadUsers)
        {
            result.AddGetLink("admin/users",
                resources.Url<UserManagementController>(x => nameof(x.GetUsers)));
        }

        result.AddGetLink("languages",
            resources.Url<LanguagesController>(x => nameof(x.GetLanguages)));

        return result;
    }
}
