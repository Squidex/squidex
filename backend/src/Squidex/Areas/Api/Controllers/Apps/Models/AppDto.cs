﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;
using Squidex.Areas.Api.Controllers.Assets;
using Squidex.Areas.Api.Controllers.Backups;
using Squidex.Areas.Api.Controllers.Ping;
using Squidex.Areas.Api.Controllers.Plans;
using Squidex.Areas.Api.Controllers.Rules;
using Squidex.Areas.Api.Controllers.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;
using Squidex.Web;
using P = Squidex.Shared.Permissions;

#pragma warning disable RECS0033 // Convert 'if' to '||' expression

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppDto : Resource
    {
        /// <summary>
        /// The name of the app.
        /// </summary>
        [LocalizedRequired]
        [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// The optional label of the app.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// The optional description of the app.
        /// </summary>
        public string? Description { get; set; }

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
        public string? PlanName { get; set; }

        /// <summary>
        /// Gets the next plan name.
        /// </summary>
        public string? PlanUpgrade { get; set; }

        public static AppDto FromApp(IAppEntity app, string userId, IAppPlansProvider plans, Resources resources)
        {
            var permissions = GetPermissions(app, userId);

            var result = SimpleMapper.Map(app, new AppDto());

            result.Permissions = permissions.ToIds();

            if (resources.Includes(P.ForApp(P.AppApi, app.Name), permissions))
            {
                result.CanAccessApi = true;
            }

            if (resources.Includes(P.ForApp(P.AppContents, app.Name), permissions))
            {
                result.CanAccessContent = true;
            }

            result.SetPlan(app, plans, resources, permissions);
            result.SetImage(app, resources);

            return result.CreateLinks(resources, permissions);
        }

        private static PermissionSet GetPermissions(IAppEntity app, string userId)
        {
            var permissions = new List<Permission>();

            if (app.Contributors.TryGetValue(userId, out var roleName) && app.Roles.TryGet(app.Name, roleName, out var role))
            {
                permissions.AddRange(role.Permissions);
            }

            return new PermissionSet(permissions);
        }

        private void SetPlan(IAppEntity app, IAppPlansProvider plans, Resources resources, PermissionSet permissions)
        {
            if (resources.IsAllowed(P.AppPlansChange, app.Name, additional: permissions))
            {
                PlanUpgrade = plans.GetPlanUpgradeForApp(app)?.Name;
            }

            PlanName = plans.GetPlanForApp(app).Plan.Name;
        }

        private void SetImage(IAppEntity app, Resources resources)
        {
            if (app.Image != null)
            {
                AddGetLink("image", resources.Url<AppsController>(x => nameof(x.GetImage), new { app = app.Name }));
            }
        }

        private AppDto CreateLinks(Resources resources, PermissionSet permissions)
        {
            var values = new { app = Name };

            AddGetLink("ping", resources.Url<PingController>(x => nameof(x.GetAppPing), values));

            if (resources.IsAllowed(P.AppDelete, Name, additional: permissions))
            {
                AddDeleteLink("delete", resources.Url<AppsController>(x => nameof(x.DeleteApp), values));
            }

            if (resources.IsAllowed(P.AppUpdateGeneral, Name, additional: permissions))
            {
                AddPutLink("update", resources.Url<AppsController>(x => nameof(x.UpdateApp), values));
            }

            if (resources.IsAllowed(P.AppUpdateImage, Name, additional: permissions))
            {
                AddPostLink("image/upload", resources.Url<AppsController>(x => nameof(x.UploadImage), values));

                AddDeleteLink("image/delete", resources.Url<AppsController>(x => nameof(x.DeleteImage), values));
            }

            if (resources.IsAllowed(P.AppAssetsRead, Name, additional: permissions))
            {
                AddGetLink("assets", resources.Url<AssetsController>(x => nameof(x.GetAssets), values));
            }

            if (resources.IsAllowed(P.AppBackupsRead, Name, additional: permissions))
            {
                AddGetLink("backups", resources.Url<BackupsController>(x => nameof(x.GetBackups), values));
            }

            if (resources.IsAllowed(P.AppClientsRead, Name, additional: permissions))
            {
                AddGetLink("clients", resources.Url<AppClientsController>(x => nameof(x.GetClients), values));
            }

            if (resources.IsAllowed(P.AppContributorsRead, Name, additional: permissions))
            {
                AddGetLink("contributors", resources.Url<AppContributorsController>(x => nameof(x.GetContributors), values));
            }

            if (resources.IsAllowed(P.AppCommon, Name, additional: permissions))
            {
                AddGetLink("languages", resources.Url<AppLanguagesController>(x => nameof(x.GetLanguages), values));
            }

            if (resources.IsAllowed(P.AppCommon, Name, additional: permissions))
            {
                AddGetLink("patterns", resources.Url<AppPatternsController>(x => nameof(x.GetPatterns), values));
            }

            if (resources.IsAllowed(P.AppPlansRead, Name, additional: permissions))
            {
                AddGetLink("plans", resources.Url<AppPlansController>(x => nameof(x.GetPlans), values));
            }

            if (resources.IsAllowed(P.AppRolesRead, Name, additional: permissions))
            {
                AddGetLink("roles", resources.Url<AppRolesController>(x => nameof(x.GetRoles), values));
            }

            if (resources.IsAllowed(P.AppRulesRead, Name, additional: permissions))
            {
                AddGetLink("rules", resources.Url<RulesController>(x => nameof(x.GetRules), values));
            }

            if (resources.IsAllowed(P.AppCommon, Name, additional: permissions))
            {
                AddGetLink("schemas", resources.Url<SchemasController>(x => nameof(x.GetSchemas), values));
            }

            if (resources.IsAllowed(P.AppWorkflowsRead, Name, additional: permissions))
            {
                AddGetLink("workflows", resources.Url<AppWorkflowsController>(x => nameof(x.GetWorkflows), values));
            }

            if (resources.IsAllowed(P.AppSchemasCreate, Name, additional: permissions))
            {
                AddPostLink("schemas/create", resources.Url<SchemasController>(x => nameof(x.PostSchema), values));
            }

            if (resources.IsAllowed(P.AppAssetsCreate, Name, additional: permissions))
            {
                AddPostLink("assets/create", resources.Url<SchemasController>(x => nameof(x.PostSchema), values));
            }

            return this;
        }
    }
}
