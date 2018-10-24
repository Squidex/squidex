// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Core
{
    public sealed class Permissions
    {
        public const string All = "squidex.*";

        public const string Admin = "squidex.admin.*";

        public const string AdminRestore = "squidex.admin.restore";
        public const string AdminRestoreRead = "squidex.admin.restore.read";
        public const string AdminRestoreCreate = "squidex.admin.restore.create";

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
        public const string AppDelete = "squidex.apps.{app}.delete";
        public const string AppCommon = "squidex.apps.{app}.common";

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
        public const string AppLanguagesRead = "squidex.apps.{app}.languages.read";
        public const string AppLanguagesCreate = "squidex.apps.{app}.languages.create";
        public const string AppLanguagesUpdate = "squidex.apps.{app}.languages.update";
        public const string AppLanguagesDelete = "squidex.apps.{app}.languages.delete";

        public const string AppPatterns = "squidex.apps.{app}.patterns";
        public const string AppPatternsRead = "squidex.apps.{app}.patterns.read";
        public const string AppPatternsCreate = "squidex.apps.{app}.patterns.create";
        public const string AppPatternsUpdate = "squidex.apps.{app}.patterns.update";
        public const string AppPatternsDelete = "squidex.apps.{app}.patterns.delete";

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
        public const string AppAssetsUpdate = "squidex.apps.{app}.assets.update";
        public const string AppAssetsDelete = "squidex.apps.{app}.assets.delete";

        public const string AppRules = "squidex.apps.{app}.rules";
        public const string AppRulesRead = "squidex.apps.{app}.rules.read";
        public const string AppRulesCreate = "squidex.apps.{app}.rules.create";
        public const string AppRulesUpdate = "squidex.apps.{app}.rules.update";
        public const string AppRulesDisable = "squidex.apps.{app}.rules.disable";
        public const string AppRulesDelete = "squidex.apps.{app}.rules.delete";

        public const string AppSchemas = "squidex.apps.{app}.schemas.{name}";
        public const string AppSchemasRead = "squidex.apps.{app}.schemas.{name}.read";
        public const string AppSchemasCreate = "squidex.apps.{app}.schemas.{name}.create";
        public const string AppSchemasUpdate = "squidex.apps.{app}.schemas.{name}.update";
        public const string AppSchemasScripts = "squidex.apps.{app}.schemas.{name}.scripts";
        public const string AppSchemasPublish = "squidex.apps.{app}.schemas.{name}.publish";
        public const string AppSchemasDelete = "squidex.apps.{app}.schemas.{name}.delete";

        public const string AppContents = "squidex.apps.{app}.contents.{name}";
        public const string AppContentsRead = "squidex.apps.{app}.contents.{name}.read";
        public const string AppContentsGraphQL = "squidex.apps.{app}.contents.{name}.graphql";
        public const string AppContentsCreate = "squidex.apps.{app}.contents.{name}.create";
        public const string AppContentsUpdate = "squidex.apps.{app}.contents.{name}.update";
        public const string AppContentsDiscard = "squidex.apps.{app}.contents.{name}.discard";
        public const string AppContentsArchive = "squidex.apps.{app}.contents.{name}.archive";
        public const string AppContentsRestore = "squidex.apps.{app}.contents.{name}.restore";
        public const string AppContentsPublish = "squidex.apps.{app}.contents.{name}.publish";
        public const string AppContentsUnpublish = "squidex.apps.{app}.contents.{name}.unpublish";
        public const string AppContentsDelete = "squidex.apps.{app}.contents.{name}.delete";

        public static Permission ForApp(string id, string app = "*")
        {
            Guard.NotNull(id, nameof(id));

            return new Permission(id.Replace("{app}", app ?? "*"));
        }

        public static Permission ForSchema(string id, string app = "*", string schema = "*")
        {
            Guard.NotNull(id, nameof(id));

            return new Permission(id.Replace("{app}", app ?? "*").Replace("{name}", schema ?? "*"));
        }
    }
}
