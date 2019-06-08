// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Backups;
using Squidex.Areas.Api.Controllers.EventConsumers;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class ResourcesDto : Resource
    {
        public static ResourcesDto FromController(ApiController controller)
        {
            var result = new ResourcesDto();

            if (controller.HasPermission(Permissions.AdminEventsRead))
            {
                result.AddGetLink("admin/eventConsumers", controller.Url<EventConsumersController>(x => nameof(x.GetEventConsumers)));
            }

            if (controller.HasPermission(Permissions.AdminRestoreRead))
            {
                result.AddGetLink("admin/restore", controller.Url<RestoreController>(x => nameof(x.GetJob)));
            }

            if (controller.HasPermission(Permissions.AdminUsersRead))
            {
                result.AddGetLink("admin/users", controller.Url<UserManagementController>(x => nameof(x.GetUsers)));
            }

            return result;
        }
    }
}
