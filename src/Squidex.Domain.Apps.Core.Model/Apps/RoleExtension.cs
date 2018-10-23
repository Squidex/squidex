// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Core.Apps
{
    public static class RoleExtension
    {
        public static PermissionSet ToPermissions(this AppClientPermission clientPermission, string app)
        {
            Guard.Enum(clientPermission, nameof(clientPermission));
            Guard.NotNullOrEmpty(app, nameof(app));

            switch (clientPermission)
            {
                case AppClientPermission.Developer:
                    return ToPermissions(AppContributorPermission.Developer, app);
                case AppClientPermission.Editor:
                    return ToPermissions(AppContributorPermission.Editor, app);
                case AppClientPermission.Reader:
                    return new PermissionSet(
                        Permissions.ForApp(Permissions.AppCommon, app),
                        Permissions.ForSchema(Permissions.AppContentsRead, app, "*"),
                        Permissions.ForSchema(Permissions.AppContentsGraphQL, app, "*"));
            }

            return PermissionSet.Empty;
        }

        public static PermissionSet ToPermissions(this AppContributorPermission contributorPermission, string app)
        {
            Guard.Enum(contributorPermission, nameof(contributorPermission));
            Guard.NotNullOrEmpty(app, nameof(app));

            switch (contributorPermission)
            {
                case AppContributorPermission.Owner:
                    return new PermissionSet(
                        Permissions.ForApp(Permissions.App, app));
                case AppContributorPermission.Developer:
                    return new PermissionSet(
                        Permissions.ForApp(Permissions.AppCommon, app),
                        Permissions.ForApp(Permissions.AppContents, app),
                        Permissions.ForApp(Permissions.AppAssets, app),
                        Permissions.ForApp(Permissions.AppPatterns, app),
                        Permissions.ForApp(Permissions.AppRules, app),
                        Permissions.ForApp(Permissions.AppSchemas, app));
                case AppContributorPermission.Editor:
                    return new PermissionSet(
                        Permissions.ForApp(Permissions.AppCommon, app),
                        Permissions.ForApp(Permissions.AppContents, app),
                        Permissions.ForApp(Permissions.AppAssets, app));
            }

            return PermissionSet.Empty;
        }
    }
}