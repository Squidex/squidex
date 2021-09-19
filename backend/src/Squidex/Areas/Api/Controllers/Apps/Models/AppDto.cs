// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

#pragma warning disable RECS0033 // Convert 'if' to '||' expression

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppDto : Resource
    {
        private static readonly JsonObject EmptyObject = JsonValue.Object();

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
        public DomainId Id { get; set; }

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
        public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Indicates if the user can access the api.
        /// </summary>
        [Obsolete("Use 'roleProperties' field now.")]
        public bool CanAccessApi { get; set; }

        /// <summary>
        /// Indicates if the user can access at least one content.
        /// </summary>
        public bool CanAccessContent { get; set; }

        /// <summary>
        /// The properties from the role.
        /// </summary>
        [LocalizedRequired]
        public JsonObject RoleProperties { get; set; } = EmptyObject;

        public static AppDto FromApp(IAppEntity app, string userId, bool isFrontend, Resources resources)
        {
            var result = SimpleMapper.Map(app, new AppDto());

            var permissions = PermissionSet.Empty;

            var isContributor = false;

            if (app.Contributors.TryGetValue(userId, out var roleName) && app.Roles.TryGet(app.Name, roleName, isFrontend, out var role))
            {
                isContributor = true;

                result.RoleProperties = role.Properties;
                result.Permissions = permissions.ToIds();

                permissions = role.Permissions;
            }

            if (resources.Includes(Shared.Permissions.ForApp(Shared.Permissions.AppContents, app.Name), permissions))
            {
                result.CanAccessContent = true;
            }

            return result.CreateLinks(app, resources, permissions, isContributor);
        }

        private AppDto CreateLinks(IAppEntity app, Resources resources, PermissionSet permissions, bool isContributor)
        {
            var values = new { app = Name };

            AddGetLink("ping", resources.Url<PingController>(x => nameof(x.GetAppPing), values));

            if (app.Image != null)
            {
                AddGetLink("image", resources.Url<AppsController>(x => nameof(x.GetImage), values));
            }

            if (isContributor)
            {
                AddDeleteLink("leave", resources.Url<AppContributorsController>(x => nameof(x.DeleteMyself), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppDelete, Name, additional: permissions))
            {
                AddDeleteLink("delete", resources.Url<AppsController>(x => nameof(x.DeleteApp), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppUpdate, Name, additional: permissions))
            {
                AddPutLink("update", resources.Url<AppsController>(x => nameof(x.PutApp), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppAssetsRead, Name, additional: permissions))
            {
                AddGetLink("assets", resources.Url<AssetsController>(x => nameof(x.GetAssets), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppBackupsRead, Name, additional: permissions))
            {
                AddGetLink("backups", resources.Url<BackupsController>(x => nameof(x.GetBackups), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppClientsRead, Name, additional: permissions))
            {
                AddGetLink("clients", resources.Url<AppClientsController>(x => nameof(x.GetClients), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppContributorsRead, Name, additional: permissions))
            {
                AddGetLink("contributors", resources.Url<AppContributorsController>(x => nameof(x.GetContributors), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppLanguagesRead, Name, additional: permissions))
            {
                AddGetLink("languages", resources.Url<AppLanguagesController>(x => nameof(x.GetLanguages), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppPlansRead, Name, additional: permissions))
            {
                AddGetLink("plans", resources.Url<AppPlansController>(x => nameof(x.GetPlans), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppRolesRead, Name, additional: permissions))
            {
                AddGetLink("roles", resources.Url<AppRolesController>(x => nameof(x.GetRoles), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppRulesRead, Name, additional: permissions))
            {
                AddGetLink("rules", resources.Url<RulesController>(x => nameof(x.GetRules), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppSchemasRead, Name, additional: permissions))
            {
                AddGetLink("schemas", resources.Url<SchemasController>(x => nameof(x.GetSchemas), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppWorkflowsRead, Name, additional: permissions))
            {
                AddGetLink("workflows", resources.Url<AppWorkflowsController>(x => nameof(x.GetWorkflows), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppSchemasCreate, Name, additional: permissions))
            {
                AddPostLink("schemas/create", resources.Url<SchemasController>(x => nameof(x.PostSchema), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppAssetsCreate, Name, additional: permissions))
            {
                AddPostLink("assets/create", resources.Url<SchemasController>(x => nameof(x.PostSchema), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppImageUpload, Name, additional: permissions))
            {
                AddPostLink("image/upload", resources.Url<AppsController>(x => nameof(x.UploadImage), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppImageDelete, Name, additional: permissions))
            {
                AddDeleteLink("image/delete", resources.Url<AppsController>(x => nameof(x.DeleteImage), values));
            }

            if (resources.IsAllowed(Shared.Permissions.AppAssetsScriptsUpdate, Name, additional: permissions))
            {
                AddDeleteLink("assets/scripts", resources.Url<AppAssetsController>(x => nameof(x.GetScripts), values));
            }

            AddGetLink("settings", resources.Url<AppSettingsController>(x => nameof(x.GetSettings), values));

            return this;
        }
    }
}
