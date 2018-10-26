// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using System.Collections.Generic;
using P = Squidex.Shared.Permissions;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class Role
    {
        public const string Editor = "Editor";
        public const string Developer = "Developer";
        public const string Owner = "Owner";
        public const string Reader = "Reader";

        private static readonly HashSet<string> DefaultRolesSet = new HashSet<string>
        {
            Editor,
            Developer,
            Owner,
            Reader
        };

        public string Name { get; }

        public PermissionSet Permissions { get; }

        public Role(string name, PermissionSet permissions)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(permissions, nameof(permissions));

            Name = name;

            Permissions = permissions;
        }

        public Role(string name, params Permission[] permissions)
            : this(name, new PermissionSet(permissions))
        {
        }

        public static bool IsDefaultRole(string role)
        {
            return role != null && DefaultRolesSet.Contains(role);
        }

        public static Role CreateOwner(string app)
        {
            return new Role(Owner,
                P.ForApp(P.App, app));
        }

        public static Role CreateEditor(string app)
        {
            return new Role(Editor,
                P.ForApp(P.AppCommon, app),
                P.ForApp(P.AppContents, app),
                P.ForApp(P.AppAssets, app));
        }

        public static Role CreateReader(string app)
        {
            return new Role(Reader,
                P.ForApp(P.AppAssetsRead, app),
                P.ForApp(P.AppCommon, app),
                P.ForApp(P.AppContentsRead, app),
                P.ForApp(P.AppContentsGraphQL, app));
        }

        public static Role CreateDeveloper(string app)
        {
            return new Role(Developer,
                P.ForApp(P.AppApi, app),
                P.ForApp(P.AppAssets, app),
                P.ForApp(P.AppCommon, app),
                P.ForApp(P.AppContents, app),
                P.ForApp(P.AppPatterns, app),
                P.ForApp(P.AppRules, app),
                P.ForApp(P.AppSchemas, app));
        }
    }
}
