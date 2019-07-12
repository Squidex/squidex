// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Areas.Api.Controllers.Assets;
using Squidex.Areas.Api.Controllers.Backups;
using Squidex.Areas.Api.Controllers.Ping;
using Squidex.Areas.Api.Controllers.Plans;
using Squidex.Areas.Api.Controllers.Rules;
using Squidex.Areas.Api.Controllers.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;
using AllPermissions = Squidex.Shared.Permissions;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppDto : Resource
    {
        /// <summary>
        /// The name of the app.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// The version of the app.
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// The id of the app.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The timestamp when the app has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The timestamp when the app has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }

        /// <summary>
        /// The permission level of the user.
        /// </summary>
        public IEnumerable<string> Permissions { get; set; }

        /// <summary>
        /// Indicates if the user can access the api.
        /// </summary>
        public bool CanAccessApi { get; set; }

        /// <summary>
        /// Indicates if the user can access at least one content.
        /// </summary>
        public bool CanAccessContent { get; set; }

        /// <summary>
        /// Gets the current plan name.
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// Gets the next plan name.
        /// </summary>
        public string PlanUpgrade { get; set; }

        public static AppDto FromApp(IAppEntity app, string userId, PermissionSet userPermissions, IAppPlansProvider plans, ApiController controller)
        {
            var permissions = GetPermissions(app, userId, userPermissions);

            var result = SimpleMapper.Map(app, new AppDto());

            result.Permissions = permissions.ToIds();
            result.PlanName = plans.GetPlanForApp(app)?.Name;

            result.CanAccessApi = controller.HasPermission(AllPermissions.AppApi, app.Name, "*", permissions);
            result.CanAccessContent = controller.HasPermission(AllPermissions.AppContentsRead, app.Name, "*", permissions);

            if (controller.HasPermission(AllPermissions.AppPlansChange, app.Name))
            {
                result.PlanUpgrade = plans.GetPlanUpgradeForApp(app)?.Name;
            }

            return result.CreateLinks(controller, permissions);
        }

        private static PermissionSet GetPermissions(IAppEntity app, string userId, PermissionSet userPermissions)
        {
            var permissions = new List<Permission>();

            if (app.Contributors.TryGetValue(userId, out var roleName) && app.Roles.TryGetValue(roleName, out var role))
            {
                permissions.AddRange(role.Permissions);
            }

            if (userPermissions != null)
            {
                permissions.AddRange(userPermissions.ToAppPermissions(app.Name));
            }

            return new PermissionSet(permissions);
        }

        private AppDto CreateLinks(ApiController controller, PermissionSet permissions)
        {
            var values = new { app = Name };

            AddGetLink("ping", controller.Url<PingController>(x => nameof(x.GetAppPing), values));

            if (controller.HasPermission(AllPermissions.AppDelete, Name, permissions: permissions))
            {
                AddDeleteLink("delete", controller.Url<AppsController>(x => nameof(x.DeleteApp), values));
            }

            if (controller.HasPermission(AllPermissions.AppAssetsRead, Name, permissions: permissions))
            {
                AddGetLink("assets", controller.Url<AssetsController>(x => nameof(x.GetAssets), values));
            }

            if (controller.HasPermission(AllPermissions.AppBackupsRead, Name, permissions: permissions))
            {
                AddGetLink("backups", controller.Url<BackupsController>(x => nameof(x.GetBackups), values));
            }

            if (controller.HasPermission(AllPermissions.AppClientsRead, Name, permissions: permissions))
            {
                AddGetLink("clients", controller.Url<AppClientsController>(x => nameof(x.GetClients), values));
            }

            if (controller.HasPermission(AllPermissions.AppContributorsRead, Name, permissions: permissions))
            {
                AddGetLink("contributors", controller.Url<AppContributorsController>(x => nameof(x.GetContributors), values));
            }

            if (controller.HasPermission(AllPermissions.AppCommon, Name, permissions: permissions))
            {
                AddGetLink("languages", controller.Url<AppLanguagesController>(x => nameof(x.GetLanguages), values));
            }

            if (controller.HasPermission(AllPermissions.AppCommon, Name, permissions: permissions))
            {
                AddGetLink("patterns", controller.Url<AppPatternsController>(x => nameof(x.GetPatterns), values));
            }

            if (controller.HasPermission(AllPermissions.AppPlansRead, Name, permissions: permissions))
            {
                AddGetLink("plans", controller.Url<AppPlansController>(x => nameof(x.GetPlans), values));
            }

            if (controller.HasPermission(AllPermissions.AppRolesRead, Name, permissions: permissions))
            {
                AddGetLink("roles", controller.Url<AppRolesController>(x => nameof(x.GetRoles), values));
            }

            if (controller.HasPermission(AllPermissions.AppRulesRead, Name, permissions: permissions))
            {
                AddGetLink("rules", controller.Url<RulesController>(x => nameof(x.GetRules), values));
            }

            if (controller.HasPermission(AllPermissions.AppCommon, Name, permissions: permissions))
            {
                AddGetLink("schemas", controller.Url<SchemasController>(x => nameof(x.GetSchemas), values));
            }

            if (controller.HasPermission(AllPermissions.AppWorkflowsRead, Name, permissions: permissions))
            {
                AddGetLink("workflows", controller.Url<AppWorkflowsController>(x => nameof(x.GetWorkflows), values));
            }

            if (controller.HasPermission(AllPermissions.AppSchemasCreate, Name, permissions: permissions))
            {
                AddPostLink("schemas/create", controller.Url<SchemasController>(x => nameof(x.PostSchema), values));
            }

            if (controller.HasPermission(AllPermissions.AppAssetsCreate, Name, permissions: permissions))
            {
                AddPostLink("assets/create", controller.Url<SchemasController>(x => nameof(x.PostSchema), values));
            }

            return this;
        }
    }
}
