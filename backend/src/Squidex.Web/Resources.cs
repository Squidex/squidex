// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Web
{
    public sealed class Resources
    {
        private readonly Dictionary<(string Id, string Schema), bool> permissions = new Dictionary<(string, string), bool>();

        // Contents
        public bool CanReadContent(string schema) => Can(Permissions.AppContentsReadOwn, schema);

        public bool CanCreateContent(string schema) => Can(Permissions.AppContentsCreate, schema);

        public bool CanCreateContentVersion(string schema) => Can(Permissions.AppContentsVersionCreateOwn, schema);

        public bool CanDeleteContent(string schema) => Can(Permissions.AppContentsDeleteOwn, schema);

        public bool CanDeleteContentVersion(string schema) => Can(Permissions.AppContentsVersionDeleteOwn, schema);

        public bool CanChangeStatus(string schema) => Can(Permissions.AppContentsChangeStatus, schema);

        public bool CanCancelContentStatus(string schema) => Can(Permissions.AppContentsChangeStatusCancelOwn, schema);

        public bool CanUpdateContent(string schema) => Can(Permissions.AppContentsUpdateOwn, schema);

        // Schemas
        public bool CanUpdateSchema(string schema) => Can(Permissions.AppSchemasDelete, schema);

        public bool CanUpdateSchemaScripts(string schema) => Can(Permissions.AppSchemasScripts, schema);

        public bool CanPublishSchema(string schema) => Can(Permissions.AppSchemasPublish, schema);

        public bool CanDeleteSchema(string schema) => Can(Permissions.AppSchemasDelete, schema);

        public bool CanCreateSchema => Can(Permissions.AppSchemasCreate);

        public bool CanUpdateSettings => Can(Permissions.AppUpdateSettings);

        // Asset Scripts
        public bool CanUpdateAssetsScripts => Can(Permissions.AppAssetsScriptsUpdate);

        // Contributors
        public bool CanAssignContributor => Can(Permissions.AppContributorsAssign);

        public bool CanRevokeContributor => Can(Permissions.AppContributorsRevoke);

        // Workflows
        public bool CanCreateWorkflow => Can(Permissions.AppWorkflowsCreate);

        public bool CanUpdateWorkflow => Can(Permissions.AppWorkflowsUpdate);

        public bool CanDeleteWorkflow => Can(Permissions.AppWorkflowsDelete);

        // Roles
        public bool CanCreateRole => Can(Permissions.AppRolesCreate);

        public bool CanUpdateRole => Can(Permissions.AppRolesUpdate);

        public bool CanDeleteRole => Can(Permissions.AppRolesDelete);

        // Languages
        public bool CanCreateLanguage => Can(Permissions.AppLanguagesCreate);

        public bool CanUpdateLanguage => Can(Permissions.AppLanguagesUpdate);

        public bool CanDeleteLanguage => Can(Permissions.AppLanguagesDelete);

        // Clients
        public bool CanCreateClient => Can(Permissions.AppClientsCreate);

        public bool CanUpdateClient => Can(Permissions.AppClientsUpdate);

        public bool CanDeleteClient => Can(Permissions.AppClientsDelete);

        // Rules
        public bool CanDisableRule => Can(Permissions.AppRulesDisable);

        public bool CanCreateRule => Can(Permissions.AppRulesCreate);

        public bool CanUpdateRule => Can(Permissions.AppRulesUpdate);

        public bool CanDeleteRule => Can(Permissions.AppRulesDelete);

        public bool CanReadRuleEvents => Can(Permissions.AppRulesEventsRead);

        public bool CanUpdateRuleEvents => Can(Permissions.AppRulesEventsUpdate);

        public bool CanRunRuleEvents => Can(Permissions.AppRulesEventsRun);

        public bool CanDeleteRuleEvents => Can(Permissions.AppRulesEventsDelete);

        // Users
        public bool CanReadUsers => Can(Permissions.AdminUsersRead);

        public bool CanCreateUser => Can(Permissions.AdminUsersCreate);

        public bool CanLockUser => Can(Permissions.AdminUsersLock);

        public bool CanUnlockUser => Can(Permissions.AdminUsersUnlock);

        public bool CanUpdateUser => Can(Permissions.AdminUsersUpdate);

        // Assets
        public bool CanUploadAsset => Can(Permissions.AppAssetsUpload);

        public bool CanCreateAsset => Can(Permissions.AppAssetsCreate);

        public bool CanDeleteAsset => Can(Permissions.AppAssetsDelete);

        public bool CanUpdateAsset => Can(Permissions.AppAssetsUpdate);

        public bool CanReadAssets => Can(Permissions.AppAssetsRead);

        // Events
        public bool CanReadEvents => Can(Permissions.AdminEventsRead);

        public bool CanManageEvents => Can(Permissions.AdminEventsManage);

        // Backups
        public bool CanRestoreBackup => Can(Permissions.AdminRestore);

        public bool CanCreateBackup => Can(Permissions.AppBackupsCreate);

        public bool CanDeleteBackup => Can(Permissions.AppBackupsDelete);

        public bool CanDownloadBackup => Can(Permissions.AppBackupsDownload);

        public Context Context { get; set; }

        public string? App => GetAppName();

        public string? Schema => GetAppName();

        public DomainId AppId => GetAppId();

        public ApiController Controller { get; }

        public Resources(ApiController controller)
        {
            Controller = controller;
            Context = controller.HttpContext.Context();
        }

        public string Url<T>(Func<T?, string> action, object? values = null) where T : ApiController
        {
            var url = Controller.Url(action, values);

            var basePath = Controller.HttpContext.Request.PathBase;

            if (url.StartsWith(Controller.HttpContext.Request.PathBase, StringComparison.OrdinalIgnoreCase))
            {
                url = url[basePath.Value!.Length..];
            }

            return url;
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

        public bool Can(string id)
        {
            return permissions.GetOrAdd((Id: id, string.Empty), k => IsAllowed(k.Id, Permission.Any, Permission.Any));
        }

        public bool Can(string id, string schema)
        {
            return permissions.GetOrAdd((Id: id, Schema: schema), k => IsAllowed(k.Id, Permission.Any, k.Schema));
        }

        public bool IsAllowed(string id, string app = Permission.Any, string schema = Permission.Any, PermissionSet? additional = null)
        {
            if (app == Permission.Any)
            {
                var fallback = GetAppName();

                if (!string.IsNullOrWhiteSpace(fallback))
                {
                    app = fallback;
                }
            }

            if (schema == Permission.Any)
            {
                var fallback = GetSchemaName();

                if (!string.IsNullOrWhiteSpace(fallback))
                {
                    schema = fallback;
                }
            }

            var permission = Permissions.ForApp(id, app, schema);

            return Context.UserPermissions.Allows(permission) || additional?.Allows(permission) == true;
        }

        private string? GetAppName()
        {
            return Controller.HttpContext.Context().App?.Name;
        }

        private string? GetSchemaName()
        {
            return Controller.HttpContext.Features.Get<ISchemaFeature>()?.Schema.SchemaDef.Name;
        }

        private DomainId GetAppId()
        {
            return Controller.HttpContext.Context().App?.Id ?? default;
        }
    }
}
