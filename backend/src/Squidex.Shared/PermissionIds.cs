// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Shared
{
    public static class PermissionIds
    {
        public const string All = "squidex.*";

        public const string Admin = "squidex.admin.*";

        // Admin App Creation
        public const string AdminAppCreate = "squidex.admin.apps.create";

        // Admin Team Creation
        public const string AdminTeamCreate = "squidex.admin.teams.create";

        // Backup Admin
        public const string AdminRestore = "squidex.admin.restore";

        // Event Admin
        public const string AdminEvents = "squidex.admin.events";
        public const string AdminEventsRead = "squidex.admin.events.read";
        public const string AdminEventsManage = "squidex.admin.events.manage";

        // User Admin
        public const string AdminUsers = "squidex.admin.users";
        public const string AdminUsersRead = "squidex.admin.users.read";
        public const string AdminUsersCreate = "squidex.admin.users.create";
        public const string AdminUsersUpdate = "squidex.admin.users.update";
        public const string AdminUsersUnlock = "squidex.admin.users.unlock";
        public const string AdminUsersLock = "squidex.admin.users.lock";

        // Team
        public const string Team = "squidex.teams.{team}";

        // Team General
        public const string TeamAdmin = "squidex.teams.{team}.*";
        public const string TeamUpdate = "squidex.teams.{team}.update";

        // Team Contributors
        public const string TeamContributors = "squidex.teams.{team}.contributors";
        public const string TeamContributorsRead = "squidex.teams.{team}.contributors.read";
        public const string TeamContributorsAssign = "squidex.teams.{team}.contributors.assign";
        public const string TeamContributorsRevoke = "squidex.teams.{team}.contributors.revoke";

        // Team Plans
        public const string TeamPlans = "squidex.teams.{team}.plans";
        public const string TeamPlansRead = "squidex.teams.{team}.plans.read";
        public const string TeamPlansChange = "squidex.teams.{team}.plans.change";

        // Team Usage
        public const string TeamUsage = "squidex.teams.{team}.usage";

        // Team History
        public const string TeamHistory = "squidex.teams.{team}.history";

        // App
        public const string App = "squidex.apps.{app}";

        // App General
        public const string AppAdmin = "squidex.apps.{app}.*";
        public const string AppDelete = "squidex.apps.{app}.delete";
        public const string AppTransfer = "squidex.apps.{app}.transfer";
        public const string AppUpdate = "squidex.apps.{app}.update";
        public const string AppUpdateSettings = "squidex.apps.{app}.settings";

        // App Image
        public const string AppImageUpload = "squidex.apps.{app}.image";
        public const string AppImageDelete = "squidex.apps.{app}.image";

        // App History
        public const string AppHistory = "squidex.apps.{app}.history";

        // App Ping
        public const string AppPing = "squidex.apps.{app}.ping";

        // App Search
        public const string AppSearch = "squidex.apps.{app}.search";

        // App Translate
        public const string AppTranslate = "squidex.apps.{app}.translate";

        // App Usage
        public const string AppUsage = "squidex.apps.{app}.usage";

        // App Comments
        public const string AppComments = "squidex.apps.{app}.comments";
        public const string AppCommentsRead = "squidex.apps.{app}.comments.read";
        public const string AppCommentsCreate = "squidex.apps.{app}.comments.create";
        public const string AppCommentsUpdate = "squidex.apps.{app}.comments.update";
        public const string AppCommentsDelete = "squidex.apps.{app}.comments.delete";

        // App Clients
        public const string AppClients = "squidex.apps.{app}.clients";
        public const string AppClientsRead = "squidex.apps.{app}.clients.read";
        public const string AppClientsCreate = "squidex.apps.{app}.clients.create";
        public const string AppClientsUpdate = "squidex.apps.{app}.clients.update";
        public const string AppClientsDelete = "squidex.apps.{app}.clients.delete";

        // App Contributors
        public const string AppContributors = "squidex.apps.{app}.contributors";
        public const string AppContributorsRead = "squidex.apps.{app}.contributors.read";
        public const string AppContributorsAssign = "squidex.apps.{app}.contributors.assign";
        public const string AppContributorsRevoke = "squidex.apps.{app}.contributors.revoke";

        // App Languages
        public const string AppLanguages = "squidex.apps.{app}.languages";
        public const string AppLanguagesRead = "squidex.apps.{app}.languages.read";
        public const string AppLanguagesCreate = "squidex.apps.{app}.languages.create";
        public const string AppLanguagesUpdate = "squidex.apps.{app}.languages.update";
        public const string AppLanguagesDelete = "squidex.apps.{app}.languages.delete";

        // App Roles
        public const string AppRoles = "squidex.apps.{app}.roles";
        public const string AppRolesRead = "squidex.apps.{app}.roles.read";
        public const string AppRolesCreate = "squidex.apps.{app}.roles.create";
        public const string AppRolesUpdate = "squidex.apps.{app}.roles.update";
        public const string AppRolesDelete = "squidex.apps.{app}.roles.delete";

        // App Workflows
        public const string AppWorkflows = "squidex.apps.{app}.workflows";
        public const string AppWorkflowsRead = "squidex.apps.{app}.workflows.read";
        public const string AppWorkflowsCreate = "squidex.apps.{app}.workflows.create";
        public const string AppWorkflowsUpdate = "squidex.apps.{app}.workflows.update";
        public const string AppWorkflowsDelete = "squidex.apps.{app}.workflows.delete";

        // App Backups
        public const string AppBackups = "squidex.apps.{app}.backups";
        public const string AppBackupsRead = "squidex.apps.{app}.backups.read";
        public const string AppBackupsCreate = "squidex.apps.{app}.backups.create";
        public const string AppBackupsDelete = "squidex.apps.{app}.backups.delete";
        public const string AppBackupsDownload = "squidex.apps.{app}.backups.download";

        // App Plans
        public const string AppPlans = "squidex.apps.{app}.plans";
        public const string AppPlansRead = "squidex.apps.{app}.plans.read";
        public const string AppPlansChange = "squidex.apps.{app}.plans.change";

        // App Assets
        public const string AppAssets = "squidex.apps.{app}.assets";
        public const string AppAssetsRead = "squidex.apps.{app}.assets.read";
        public const string AppAssetsCreate = "squidex.apps.{app}.assets.create";
        public const string AppAssetsUpload = "squidex.apps.{app}.assets.upload";
        public const string AppAssetsUpdate = "squidex.apps.{app}.assets.update";
        public const string AppAssetsDelete = "squidex.apps.{app}.assets.delete";

        // App Asset Scripts
        public const string AppAssetScripts = "squidex.apps.{app}.asset-scripts";
        public const string AppAssetSScriptsRead = "squidex.apps.{app}.asset-scripts.read";
        public const string AppAssetsScriptsUpdate = "squidex.apps.{app}.asset-scripts.update";

        // App Rules
        public const string AppRules = "squidex.apps.{app}.rules";
        public const string AppRulesRead = "squidex.apps.{app}.rules.read";
        public const string AppRulesCreate = "squidex.apps.{app}.rules.create";
        public const string AppRulesUpdate = "squidex.apps.{app}.rules.update";
        public const string AppRulesDisable = "squidex.apps.{app}.rules.disable";
        public const string AppRulesDelete = "squidex.apps.{app}.rules.delete";

        // App Rule Events
        public const string AppRulesEvents = "squidex.apps.{app}.rules.events";
        public const string AppRulesEventsRun = "squidex.apps.{app}.rules.events.run";
        public const string AppRulesEventsRead = "squidex.apps.{app}.rules.events.read";
        public const string AppRulesEventsUpdate = "squidex.apps.{app}.rules.events.update";
        public const string AppRulesEventsDelete = "squidex.apps.{app}.rules.events.delete";

        // App Schemas
        public const string AppSchemas = "squidex.apps.{app}.schemas";
        public const string AppSchemasRead = "squidex.apps.{app}.schemas.read";
        public const string AppSchemasCreate = "squidex.apps.{app}.schemas.create";
        public const string AppSchemasUpdate = "squidex.apps.{app}.schemas.{schema}.update";
        public const string AppSchemasScripts = "squidex.apps.{app}.schemas.{schema}.scripts";
        public const string AppSchemasPublish = "squidex.apps.{app}.schemas.{schema}.publish";
        public const string AppSchemasDelete = "squidex.apps.{app}.schemas.{schema}.delete";

        // App Contents
        public const string AppContents = "squidex.apps.{app}.contents.{schema}";
        public const string AppContentsRead = "squidex.apps.{app}.contents.{schema}.read";
        public const string AppContentsReadOwn = "squidex.apps.{app}.contents.{schema}.read.own";
        public const string AppContentsCreate = "squidex.apps.{app}.contents.{schema}.create";
        public const string AppContentsUpdate = "squidex.apps.{app}.contents.{schema}.update";
        public const string AppContentsUpdateOwn = "squidex.apps.{app}.contents.{schema}.update.own";
        public const string AppContentsChangeStatusCancel = "squidex.apps.{app}.contents.{schema}.changestatus.cancel";
        public const string AppContentsChangeStatusCancelOwn = "squidex.apps.{app}.contents.{schema}.changestatus.cancel.own";
        public const string AppContentsChangeStatus = "squidex.apps.{app}.contents.{schema}.changestatus";
        public const string AppContentsChangeStatusOwn = "squidex.apps.{app}.contents.{schema}.changestatus.own";
        public const string AppContentsUpsert = "squidex.apps.{app}.contents.{schema}.upsert";
        public const string AppContentsVersionCreate = "squidex.apps.{app}.contents.{schema}.version.create";
        public const string AppContentsVersionCreateOwn = "squidex.apps.{app}.contents.{schema}.version.create.own";
        public const string AppContentsVersionDelete = "squidex.apps.{app}.contents.{schema}.version.delete";
        public const string AppContentsVersionDeleteOwn = "squidex.apps.{app}.contents.{schema}.version.delete.own";
        public const string AppContentsDelete = "squidex.apps.{app}.contents.{schema}.delete";
        public const string AppContentsDeleteOwn = "squidex.apps.{app}.contents.{schema}.delete.own";

        public static Permission ForApp(string id, string app = Permission.Any, string schema = Permission.Any, string team = Permission.Any)
        {
            Guard.NotNull(id);

            id = id.Replace("{app}", app ?? Permission.Any, StringComparison.Ordinal);
            id = id.Replace("{schema}", schema ?? Permission.Any, StringComparison.Ordinal);
            id = id.Replace("{team}", team ?? Permission.Any, StringComparison.Ordinal);

            return new Permission(id);
        }
    }
}
