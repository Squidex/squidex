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
using P = Squidex.Shared.Permissions;

namespace Squidex.Web
{
    public sealed class Resources
    {
        private readonly Dictionary<(string, string), bool> schemaPermissions = new Dictionary<(string, string), bool>();

        // Contents
        public bool CanReadContent(string schema) => IsAllowedForSchema(P.AppContentsReadOwn, schema);

        public bool CanCreateContent(string schema) => IsAllowedForSchema(P.AppContentsCreate, schema);

        public bool CanCreateContentVersion(string schema) => IsAllowedForSchema(P.AppContentsVersionCreateOwn, schema);

        public bool CanDeleteContent(string schema) => IsAllowedForSchema(P.AppContentsDeleteOwn, schema);

        public bool CanDeleteContentVersion(string schema) => IsAllowedForSchema(P.AppContentsVersionDeleteOwn, schema);

        public bool CanUpdateContent(string schema) => IsAllowedForSchema(P.AppContentsUpdateOwn, schema);

        // Schemas
        public bool CanUpdateSchema(string schema) => IsAllowedForSchema(P.AppSchemasDelete, schema);

        public bool CanUpdateSchemaScripts(string schema) => IsAllowedForSchema(P.AppSchemasScripts, schema);

        public bool CanPublishSchema(string schema) => IsAllowedForSchema(P.AppSchemasPublish, schema);

        public bool CanDeleteSchema(string schema) => IsAllowedForSchema(P.AppSchemasDelete, schema);

        [Lazy]
        public bool CanCreateSchema => IsAllowed(P.AppSchemasCreate);

        // Contributors
        [Lazy]
        public bool CanAssignContributor => IsAllowed(P.AppContributorsAssign);

        [Lazy]
        public bool CanRevokeContributor => IsAllowed(P.AppContributorsRevoke);

        // Workflows
        [Lazy]
        public bool CanCreateWorkflow => IsAllowed(P.AppWorkflowsCreate);

        [Lazy]
        public bool CanUpdateWorkflow => IsAllowed(P.AppWorkflowsUpdate);

        [Lazy]
        public bool CanDeleteWorkflow => IsAllowed(P.AppWorkflowsDelete);

        // Roles
        [Lazy]
        public bool CanCreateRole => IsAllowed(P.AppRolesCreate);

        [Lazy]
        public bool CanUpdateRole => IsAllowed(P.AppRolesUpdate);

        [Lazy]
        public bool CanDeleteRole => IsAllowed(P.AppRolesDelete);

        // Languages
        [Lazy]
        public bool CanCreateLanguage => IsAllowed(P.AppLanguagesCreate);

        [Lazy]
        public bool CanUpdateLanguage => IsAllowed(P.AppLanguagesUpdate);

        [Lazy]
        public bool CanDeleteLanguage => IsAllowed(P.AppLanguagesDelete);

        // Patterns
        [Lazy]
        public bool CanCreatePattern => IsAllowed(P.AppClientsCreate);

        [Lazy]
        public bool CanUpdatePattern => IsAllowed(P.AppPatternsUpdate);

        [Lazy]
        public bool CanDeletePattern => IsAllowed(P.AppPatternsDelete);

        // Clients
        [Lazy]
        public bool CanCreateClient => IsAllowed(P.AppClientsCreate);

        [Lazy]
        public bool CanUpdateClient => IsAllowed(P.AppClientsUpdate);

        [Lazy]
        public bool CanDeleteClient => IsAllowed(P.AppClientsDelete);

        // Rules
        [Lazy]
        public bool CanDisableRule => IsAllowed(P.AppRulesDisable);

        [Lazy]
        public bool CanCreateRule => IsAllowed(P.AppRulesCreate);

        [Lazy]
        public bool CanUpdateRule => IsAllowed(P.AppRulesUpdate);

        [Lazy]
        public bool CanDeleteRule => IsAllowed(P.AppRulesDelete);

        [Lazy]
        public bool CanReadRuleEvents => IsAllowed(P.AppRulesEvents);

        // Users
        [Lazy]
        public bool CanReadUsers => IsAllowed(P.AdminUsersRead);

        [Lazy]
        public bool CanCreateUser => IsAllowed(P.AdminUsersCreate);

        [Lazy]
        public bool CanLockUser => IsAllowed(P.AdminUsersLock);

        [Lazy]
        public bool CanUnlockUser => IsAllowed(P.AdminUsersUnlock);

        [Lazy]
        public bool CanUpdateUser => IsAllowed(P.AdminUsersUpdate);

        // Assets
        [Lazy]
        public bool CanUploadAsset => IsAllowed(P.AppAssetsUpload);

        [Lazy]
        public bool CanCreateAsset => IsAllowed(P.AppAssetsCreate);

        [Lazy]
        public bool CanDeleteAsset => IsAllowed(P.AppAssetsDelete);

        [Lazy]
        public bool CanUpdateAsset => IsAllowed(P.AppAssetsUpdate);

        [Lazy]
        public bool CanReadAssets => IsAllowed(P.AppAssetsRead);

        // Events
        [Lazy]
        public bool CanReadEvents => IsAllowed(P.AdminEventsRead);

        [Lazy]
        public bool CanManageEvents => IsAllowed(P.AdminEventsManage);

        // Orleans
        [Lazy]
        public bool CanReadOrleans => IsAllowed(P.AdminOrleans);

        // Backups
        [Lazy]
        public bool CanRestoreBackup => IsAllowed(P.AdminEventsRead);

        [Lazy]
        public bool CanCreateBackup => IsAllowed(P.AppBackupsCreate);

        [Lazy]
        public bool CanDeleteBackup => IsAllowed(P.AppBackupsDelete);

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
                var falback = Controller.HttpContext.Features.Get<ISchemaFeature>()?.SchemaId.Name;

                if (!string.IsNullOrWhiteSpace(falback))
                {
                    schema = falback;
                }
            }

            var permission = P.ForApp(id, app, schema);

            return Context.UserPermissions.Allows(permission) || additional?.Allows(permission) == true;
        }

        private string? GetAppName()
        {
            return Controller.HttpContext.Context().App?.Name;
        }
    }
}
