// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Lazy;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Web
{
    public sealed class Resources
    {
        private readonly Dictionary<(string, string), bool> schemaPermissions = new Dictionary<(string, string), bool>();

        // Contents
        public bool CanReadContent(string schema) => IsAllowedForSchema(Permissions.AppContentsReadOwn, schema);

        public bool CanCreateContent(string schema) => IsAllowedForSchema(Permissions.AppContentsCreate, schema);

        public bool CanCreateContentVersion(string schema) => IsAllowedForSchema(Permissions.AppContentsVersionCreateOwn, schema);

        public bool CanDeleteContent(string schema) => IsAllowedForSchema(Permissions.AppContentsDeleteOwn, schema);

        public bool CanDeleteContentVersion(string schema) => IsAllowedForSchema(Permissions.AppContentsVersionDeleteOwn, schema);

        public bool CanChangeStatus(string schema) => IsAllowedForSchema(Permissions.AppContentsChangeStatus, schema);

        public bool CanCancelContentStatus(string schema) => IsAllowedForSchema(Permissions.AppContentsChangeStatusCancelOwn, schema);

        public bool CanUpdateContent(string schema) => IsAllowedForSchema(Permissions.AppContentsUpdateOwn, schema);

        // Schemas
        public bool CanUpdateSchema(string schema) => IsAllowedForSchema(Permissions.AppSchemasDelete, schema);

        public bool CanUpdateSchemaScripts(string schema) => IsAllowedForSchema(Permissions.AppSchemasScripts, schema);

        public bool CanPublishSchema(string schema) => IsAllowedForSchema(Permissions.AppSchemasPublish, schema);

        public bool CanDeleteSchema(string schema) => IsAllowedForSchema(Permissions.AppSchemasDelete, schema);

        [Lazy]
        public bool CanCreateSchema => IsAllowed(Permissions.AppSchemasCreate);

        [Lazy]
        public bool CanUpdateSettings => IsAllowed(Permissions.AppUpdateSettings);

        // Asset Scripts
        [Lazy]
        public bool CanUpdateAssetsScripts => IsAllowed(Permissions.AppAssetsScriptsUpdate);

        // Contributors
        [Lazy]
        public bool CanAssignContributor => IsAllowed(Permissions.AppContributorsAssign);

        [Lazy]
        public bool CanRevokeContributor => IsAllowed(Permissions.AppContributorsRevoke);

        // Workflows
        [Lazy]
        public bool CanCreateWorkflow => IsAllowed(Permissions.AppWorkflowsCreate);

        [Lazy]
        public bool CanUpdateWorkflow => IsAllowed(Permissions.AppWorkflowsUpdate);

        [Lazy]
        public bool CanDeleteWorkflow => IsAllowed(Permissions.AppWorkflowsDelete);

        // Roles
        [Lazy]
        public bool CanCreateRole => IsAllowed(Permissions.AppRolesCreate);

        [Lazy]
        public bool CanUpdateRole => IsAllowed(Permissions.AppRolesUpdate);

        [Lazy]
        public bool CanDeleteRole => IsAllowed(Permissions.AppRolesDelete);

        // Languages
        [Lazy]
        public bool CanCreateLanguage => IsAllowed(Permissions.AppLanguagesCreate);

        [Lazy]
        public bool CanUpdateLanguage => IsAllowed(Permissions.AppLanguagesUpdate);

        [Lazy]
        public bool CanDeleteLanguage => IsAllowed(Permissions.AppLanguagesDelete);

        // Clients
        [Lazy]
        public bool CanCreateClient => IsAllowed(Permissions.AppClientsCreate);

        [Lazy]
        public bool CanUpdateClient => IsAllowed(Permissions.AppClientsUpdate);

        [Lazy]
        public bool CanDeleteClient => IsAllowed(Permissions.AppClientsDelete);

        // Rules
        [Lazy]
        public bool CanDisableRule => IsAllowed(Permissions.AppRulesDisable);

        [Lazy]
        public bool CanCreateRule => IsAllowed(Permissions.AppRulesCreate);

        [Lazy]
        public bool CanUpdateRule => IsAllowed(Permissions.AppRulesUpdate);

        [Lazy]
        public bool CanDeleteRule => IsAllowed(Permissions.AppRulesDelete);

        [Lazy]
        public bool CanReadRuleEvents => IsAllowed(Permissions.AppRulesEvents);

        // Users
        [Lazy]
        public bool CanReadUsers => IsAllowed(Permissions.AdminUsersRead);

        [Lazy]
        public bool CanCreateUser => IsAllowed(Permissions.AdminUsersCreate);

        [Lazy]
        public bool CanLockUser => IsAllowed(Permissions.AdminUsersLock);

        [Lazy]
        public bool CanUnlockUser => IsAllowed(Permissions.AdminUsersUnlock);

        [Lazy]
        public bool CanUpdateUser => IsAllowed(Permissions.AdminUsersUpdate);

        // Assets
        [Lazy]
        public bool CanUploadAsset => IsAllowed(Permissions.AppAssetsUpload);

        [Lazy]
        public bool CanCreateAsset => IsAllowed(Permissions.AppAssetsCreate);

        [Lazy]
        public bool CanDeleteAsset => IsAllowed(Permissions.AppAssetsDelete);

        [Lazy]
        public bool CanUpdateAsset => IsAllowed(Permissions.AppAssetsUpdate);

        [Lazy]
        public bool CanReadAssets => IsAllowed(Permissions.AppAssetsRead);

        // Events
        [Lazy]
        public bool CanReadEvents => IsAllowed(Permissions.AdminEventsRead);

        [Lazy]
        public bool CanManageEvents => IsAllowed(Permissions.AdminEventsManage);

        // Orleans
        [Lazy]
        public bool CanReadOrleans => IsAllowed(Permissions.AdminOrleans);

        // Backups
        [Lazy]
        public bool CanRestoreBackup => IsAllowed(Permissions.AdminRestore);

        [Lazy]
        public bool CanCreateBackup => IsAllowed(Permissions.AppBackupsCreate);

        [Lazy]
        public bool CanDeleteBackup => IsAllowed(Permissions.AppBackupsDelete);

        [Lazy]
        public string? App => GetAppName();

        public ApiController Controller { get; }

        public Context Context { get; set; }

        public Resources(ApiController controller)
        {
            Controller = controller;

            Context = controller.HttpContext.Context();
        }

        public string Url<T>(Func<T?, string> action, object? values = null) where T : ApiController
        {
            return Controller.Url(action, values);
        }

        public bool IsUser(string userId)
        {
            var subject = Controller.User.OpenIdSubject();

            return string.Equals(subject, userId, StringComparison.OrdinalIgnoreCase);
        }

        public bool Includes(Permission permission, PermissionSet? additional = null)
        {
            return Context.UserPermissions.Includes(permission) || additional?.Includes(permission) == true;
        }

        public bool IsAllowedForSchema(string id, string schema)
        {
            return schemaPermissions.GetOrAdd((id, schema), k => IsAllowed(k.Item1, "*", k.Item2));
        }

        public bool IsAllowed(string id, string app = Permission.Any, string schema = Permission.Any, PermissionSet? additional = null)
        {
            if (app == Permission.Any)
            {
                var falback = App;

                if (!string.IsNullOrWhiteSpace(falback))
                {
                    app = falback;
                }
            }

            if (schema == Permission.Any)
            {
                var falback = Controller.HttpContext.Features.Get<ISchemaFeature>()?.Schema.SchemaDef.Name;

                if (!string.IsNullOrWhiteSpace(falback))
                {
                    schema = falback;
                }
            }

            var permission = Permissions.ForApp(id, app, schema);

            return Context.UserPermissions.Allows(permission) || additional?.Allows(permission) == true;
        }

        private string? GetAppName()
        {
            return Controller.HttpContext.Context().App?.Name;
        }
    }
}
