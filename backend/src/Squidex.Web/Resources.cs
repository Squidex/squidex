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

namespace Squidex.Web;

public sealed class Resources
{
    private readonly Dictionary<(string Id, string Schema), bool> permissions = new Dictionary<(string, string), bool>();

    // Contents
    public bool CanReadContent(string schema) => Can(PermissionIds.AppContentsReadOwn, schema);

    public bool CanCreateContent(string schema) => Can(PermissionIds.AppContentsCreate, schema);

    public bool CanCreateContentVersion(string schema) => Can(PermissionIds.AppContentsVersionCreateOwn, schema);

    public bool CanDeleteContent(string schema) => Can(PermissionIds.AppContentsDeleteOwn, schema);

    public bool CanDeleteContentVersion(string schema) => Can(PermissionIds.AppContentsVersionDeleteOwn, schema);

    public bool CanChangeStatus(string schema) => Can(PermissionIds.AppContentsChangeStatus, schema);

    public bool CanCancelContentStatus(string schema) => Can(PermissionIds.AppContentsChangeStatusCancelOwn, schema);

    public bool CanUpdateContent(string schema) => Can(PermissionIds.AppContentsUpdateOwn, schema);

    // Schemas
    public bool CanUpdateSchema(string schema) => Can(PermissionIds.AppSchemasDelete, schema);

    public bool CanUpdateSchemaScripts(string schema) => Can(PermissionIds.AppSchemasScripts, schema);

    public bool CanPublishSchema(string schema) => Can(PermissionIds.AppSchemasPublish, schema);

    public bool CanDeleteSchema(string schema) => Can(PermissionIds.AppSchemasDelete, schema);

    public bool CanCreateSchema => Can(PermissionIds.AppSchemasCreate);

    public bool CanUpdateSettings => Can(PermissionIds.AppUpdateSettings);

    // Asset Scripts
    public bool CanUpdateAssetsScripts => Can(PermissionIds.AppAssetsScriptsUpdate);

    // Contributors
    public bool CanAssignContributor => Can(PermissionIds.AppContributorsAssign);

    public bool CanAssignTeamContributor => Can(PermissionIds.TeamContributorsAssign);

    public bool CanRevokeContributor => Can(PermissionIds.AppContributorsRevoke);

    public bool CanRevokeTeamContributor => Can(PermissionIds.TeamContributorsRevoke);

    // Workflows
    public bool CanCreateWorkflow => Can(PermissionIds.AppWorkflowsCreate);

    public bool CanUpdateWorkflow => Can(PermissionIds.AppWorkflowsUpdate);

    public bool CanDeleteWorkflow => Can(PermissionIds.AppWorkflowsDelete);

    // Roles
    public bool CanCreateRole => Can(PermissionIds.AppRolesCreate);

    public bool CanUpdateRole => Can(PermissionIds.AppRolesUpdate);

    public bool CanDeleteRole => Can(PermissionIds.AppRolesDelete);

    // Languages
    public bool CanCreateLanguage => Can(PermissionIds.AppLanguagesCreate);

    public bool CanUpdateLanguage => Can(PermissionIds.AppLanguagesUpdate);

    public bool CanDeleteLanguage => Can(PermissionIds.AppLanguagesDelete);

    // Clients
    public bool CanCreateClient => Can(PermissionIds.AppClientsCreate);

    public bool CanUpdateClient => Can(PermissionIds.AppClientsUpdate);

    public bool CanDeleteClient => Can(PermissionIds.AppClientsDelete);

    // Rules
    public bool CanDisableRule => Can(PermissionIds.AppRulesDisable);

    public bool CanCreateRule => Can(PermissionIds.AppRulesCreate);

    public bool CanUpdateRule => Can(PermissionIds.AppRulesUpdate);

    public bool CanDeleteRule => Can(PermissionIds.AppRulesDelete);

    public bool CanReadRuleEvents => Can(PermissionIds.AppRulesEventsRead);

    public bool CanUpdateRuleEvents => Can(PermissionIds.AppRulesEventsUpdate);

    public bool CanRunRuleEvents => Can(PermissionIds.AppRulesEventsRun);

    public bool CanDeleteRuleEvents => Can(PermissionIds.AppRulesEventsDelete);

    // Users
    public bool CanReadUsers => Can(PermissionIds.AdminUsersRead);

    public bool CanCreateUser => Can(PermissionIds.AdminUsersCreate);

    public bool CanLockUser => Can(PermissionIds.AdminUsersLock);

    public bool CanUnlockUser => Can(PermissionIds.AdminUsersUnlock);

    public bool CanUpdateUser => Can(PermissionIds.AdminUsersUpdate);

    // Assets
    public bool CanUploadAsset => Can(PermissionIds.AppAssetsUpload);

    public bool CanCreateAsset => Can(PermissionIds.AppAssetsCreate);

    public bool CanDeleteAsset => Can(PermissionIds.AppAssetsDelete);

    public bool CanUpdateAsset => Can(PermissionIds.AppAssetsUpdate);

    public bool CanReadAssets => Can(PermissionIds.AppAssetsRead);

    // Events
    public bool CanReadEvents => Can(PermissionIds.AdminEventsRead);

    public bool CanManageEvents => Can(PermissionIds.AdminEventsManage);

    // Plans
    public bool CanChangePlan => Can(PermissionIds.AppPlansChange);

    public bool CanChangeTeamPlan => Can(PermissionIds.TeamPlansChange);

    // Backups
    public bool CanRestoreBackup => Can(PermissionIds.AdminRestore);

    public bool CanCreateBackup => Can(PermissionIds.AppBackupsCreate);

    public bool CanDeleteBackup => Can(PermissionIds.AppBackupsDelete);

    public bool CanDownloadBackup => Can(PermissionIds.AppBackupsDownload);

    public Context Context { get; set; }

    public string? App => GetAppName();

    public string? Schema => GetAppName();

    public string? Team => GetTeamId().ToString();

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

    public bool IsAllowed(string id, string app = Permission.Any, string schema = Permission.Any, string team = Permission.Any, PermissionSet? additional = null)
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

        if (team == Permission.Any)
        {
            var fallback = GetTeamId();

            if (fallback != default)
            {
                team = fallback.ToString();
            }
        }

        var permission = PermissionIds.ForApp(id, app, schema, team);

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

    private DomainId GetTeamId()
    {
        return Controller.HttpContext.Features.Get<ITeamFeature>()?.Team?.Id ?? default;
    }
}
