// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Shared
{
    public static class Permissions
    {
        private static readonly List<string> ForAppsNonSchemaList = new List<string>();
        private static readonly List<string> ForAppsSchemaList = new List<string>();

        public static IReadOnlyList<string> ForAppsNonSchema
        {
            get { return ForAppsNonSchemaList; }
        }

        public static IReadOnlyList<string> ForAppsSchema
        {
            get { return ForAppsSchemaList; }
        }

        public const string All = "squidex.*";

        public const string Admin = "squidex.admin.*";
        public const string AdminOrleans = "squidex.admin.orleans";

        public const string AdminAppCreate = "squidex.admin.apps.create";

        public const string AdminRestore = "squidex.admin.restore";

        public const string AdminEvents = "squidex.admin.events";
        public const string AdminEventsRead = "squidex.admin.events.read";
        public const string AdminEventsManage = "squidex.admin.events.manage";

        public const string AdminUsers = "squidex.admin.users";
        public const string AdminUsersRead = "squidex.admin.users.read";
        public const string AdminUsersCreate = "squidex.admin.users.create";
        public const string AdminUsersUpdate = "squidex.admin.users.update";
        public const string AdminUsersUnlock = "squidex.admin.users.unlock";
        public const string AdminUsersLock = "squidex.admin.users.lock";

        public const string App = "squidex.apps.{app}";
        public const string AppCommon = "squidex.apps.{app}.common";

        public const string AppDelete = "squidex.apps.{app}.delete";
        public const string AppUpdate = "squidex.apps.{app}.update";
        public const string AppUpdateImage = "squidex.apps.{app}.update";
        public const string AppUpdateGeneral = "squidex.apps.{app}.general";

        public const string AppClients = "squidex.apps.{app}.clients";
        public const string AppClientsRead = "squidex.apps.{app}.clients.read";
        public const string AppClientsCreate = "squidex.apps.{app}.clients.create";
        public const string AppClientsUpdate = "squidex.apps.{app}.clients.update";
        public const string AppClientsDelete = "squidex.apps.{app}.clients.delete";

        public const string AppContributors = "squidex.apps.{app}.contributors";
        public const string AppContributorsRead = "squidex.apps.{app}.contributors.read";
        public const string AppContributorsAssign = "squidex.apps.{app}.contributors.assign";
        public const string AppContributorsRevoke = "squidex.apps.{app}.contributors.revoke";

        public const string AppLanguages = "squidex.apps.{app}.languages";
        public const string AppLanguagesCreate = "squidex.apps.{app}.languages.create";
        public const string AppLanguagesUpdate = "squidex.apps.{app}.languages.update";
        public const string AppLanguagesDelete = "squidex.apps.{app}.languages.delete";

        public const string AppRoles = "squidex.apps.{app}.roles";
        public const string AppRolesRead = "squidex.apps.{app}.roles.read";
        public const string AppRolesCreate = "squidex.apps.{app}.roles.create";
        public const string AppRolesUpdate = "squidex.apps.{app}.roles.update";
        public const string AppRolesDelete = "squidex.apps.{app}.roles.delete";

        public const string AppPatterns = "squidex.apps.{app}.patterns";
        public const string AppPatternsCreate = "squidex.apps.{app}.patterns.create";
        public const string AppPatternsUpdate = "squidex.apps.{app}.patterns.update";
        public const string AppPatternsDelete = "squidex.apps.{app}.patterns.delete";

        public const string AppWorkflows = "squidex.apps.{app}.workflows";
        public const string AppWorkflowsRead = "squidex.apps.{app}.workflows.read";
        public const string AppWorkflowsCreate = "squidex.apps.{app}.workflows.create";
        public const string AppWorkflowsUpdate = "squidex.apps.{app}.workflows.update";
        public const string AppWorkflowsDelete = "squidex.apps.{app}.workflows.delete";

        public const string AppBackups = "squidex.apps.{app}.backups";
        public const string AppBackupsRead = "squidex.apps.{app}.backups.read";
        public const string AppBackupsCreate = "squidex.apps.{app}.backups.create";
        public const string AppBackupsDelete = "squidex.apps.{app}.backups.delete";

        public const string AppPlans = "squidex.apps.{app}.plans";
        public const string AppPlansRead = "squidex.apps.{app}.plans.read";
        public const string AppPlansChange = "squidex.apps.{app}.plans.change";

        public const string AppAssets = "squidex.apps.{app}.assets";
        public const string AppAssetsRead = "squidex.apps.{app}.assets.read";
        public const string AppAssetsCreate = "squidex.apps.{app}.assets.create";
        public const string AppAssetsUpload = "squidex.apps.{app}.assets.upload";
        public const string AppAssetsUpdate = "squidex.apps.{app}.assets.update";
        public const string AppAssetsDelete = "squidex.apps.{app}.assets.delete";

        public const string AppRules = "squidex.apps.{app}.rules";
        public const string AppRulesRead = "squidex.apps.{app}.rules.read";
        public const string AppRulesEvents = "squidex.apps.{app}.rules.events";
        public const string AppRulesCreate = "squidex.apps.{app}.rules.create";
        public const string AppRulesUpdate = "squidex.apps.{app}.rules.update";
        public const string AppRulesDisable = "squidex.apps.{app}.rules.disable";
        public const string AppRulesDelete = "squidex.apps.{app}.rules.delete";

        public const string AppSchemas = "squidex.apps.{app}.schemas.{name}";
        public const string AppSchemasCreate = "squidex.apps.{app}.schemas.{name}.create";
        public const string AppSchemasUpdate = "squidex.apps.{app}.schemas.{name}.update";
        public const string AppSchemasScripts = "squidex.apps.{app}.schemas.{name}.scripts";
        public const string AppSchemasPublish = "squidex.apps.{app}.schemas.{name}.publish";
        public const string AppSchemasDelete = "squidex.apps.{app}.schemas.{name}.delete";

        public const string AppContents = "squidex.apps.{app}.contents.{name}";
        public const string AppContentsRead = "squidex.apps.{app}.contents.{name}.read";
        public const string AppContentsCreate = "squidex.apps.{app}.contents.{name}.create";
        public const string AppContentsUpdate = "squidex.apps.{app}.contents.{name}.update";
        public const string AppContentsUpdatePartial = "squidex.apps.{app}.contents.{name}.update.partial";
        public const string AppContentsVersionCreate = "squidex.apps.{app}.contents.{name}.version.create";
        public const string AppContentsVersionDelete = "squidex.apps.{app}.contents.{name}.version.delete";
        public const string AppContentsDelete = "squidex.apps.{app}.contents.{name}.delete";

        public const string AppApi = "squidex.apps.{app}.api";

        static Permissions()
        {
            foreach (var field in typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    var value = field.GetValue(null) as string;

                    if (value?.StartsWith(App, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (value.IndexOf("{name}", App.Length, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            ForAppsSchemaList.Add(value);
                        }
                        else
                        {
                            ForAppsNonSchemaList.Add(value);
                        }
                    }
                }
            }
        }

        public static Permission ForApp(string id, string app = Permission.Any, string schema = Permission.Any)
        {
            Guard.NotNull(id);

            return new Permission(id.Replace("{app}", app ?? Permission.Any).Replace("{name}", schema ?? Permission.Any));
        }

        public static PermissionSet ToAppPermissions(this PermissionSet permissions, string app)
        {
            var matching = permissions.Where(x => x.StartsWith($"squidex.apps.{app}"));

            return new PermissionSet(matching);
        }

        public static string[] ToAppNames(this PermissionSet permissions)
        {
            var matching = permissions.Where(x => x.StartsWith("squidex.apps."));

            var result =
                matching
                    .Select(x => x.Id.Split('.')).Where(x => x.Length > 2)
                    .Select(x => x[2])
                    .Distinct()
                    .ToArray();

            return result;
        }
    }
}
