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
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class ResourcesDto : Resource
    {
        public static ResourcesDto FromController(ApiController controller)
        {
            var result = new ResourcesDto();

            result.AddGetLink("ping", controller.Url<PingController>(x => nameof(x.GetPing)));

            if (controller.HasPermission(Permissions.AdminEventsRead))
            {
                result.AddGetLink("admin/events", controller.Url<EventConsumersController>(x => nameof(x.GetEventConsumers)));
            }

            if (controller.HasPermission(Permissions.AdminRestore))
            {
                result.AddGetLink("admin/restore", controller.Url<RestoreController>(x => nameof(x.GetJob)));
            }

            if (controller.HasPermission(Permissions.AdminUsersRead))
            {
                result.AddGetLink("admin/users", controller.Url<UserManagementController>(x => nameof(x.GetUsers)));
            }

            result.AddGetLink("languages", controller.Url<LanguagesController>(x => nameof(x.GetLanguages)));

            return result;
        }
    }
}
