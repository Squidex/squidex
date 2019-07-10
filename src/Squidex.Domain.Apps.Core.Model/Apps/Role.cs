// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using P = Squidex.Shared.Permissions;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class Role : Named
    {
        public const string Editor = "Editor";
        public const string Developer = "Developer";
        public const string Owner = "Owner";
        public const string Reader = "Reader";

        private static readonly HashSet<string> DefaultRolesSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Editor,
            Developer,
            Owner,
            Reader
        };

        public PermissionSet Permissions { get; }

        public Role(string name, PermissionSet permissions)
            : base(name)
        {
            Guard.NotNull(permissions, nameof(permissions));

            Permissions = permissions;
        }

        public Role(string name, params Permission[] permissions)
            : this(name, new PermissionSet(permissions))
        {
        }

        [Pure]
        public Role Update(string[] permissions)
        {
            return new Role(Name, new PermissionSet(permissions));
        }

        public static bool IsDefaultRole(string role)
        {
            return role != null && DefaultRolesSet.Contains(role);
        }

        public static bool IsRole(string name, string expected)
        {
            return name != null && string.Equals(name, expected, StringComparison.OrdinalIgnoreCase);
        }

        public static Role CreateOwner(string app)
        {
            return new Role(Owner,
                P.ForApp(P.App, app));
        }

        public static Role CreateEditor(string app)
        {
            return new Role(Editor,
                P.ForApp(P.AppAssets, app),
                P.ForApp(P.AppCommon, app),
                P.ForApp(P.AppContents, app),
                P.ForApp(P.AppWorkflowsRead, app));
        }

        public static Role CreateReader(string app)
        {
            return new Role(Reader,
                P.ForApp(P.AppAssetsRead, app),
                P.ForApp(P.AppCommon, app),
                P.ForApp(P.AppContentsRead, app));
        }

        public static Role CreateDeveloper(string app)
        {
            return new Role(Developer,
                P.ForApp(P.AppApi, app),
                P.ForApp(P.AppAssets, app),
                P.ForApp(P.AppCommon, app),
                P.ForApp(P.AppContents, app),
                P.ForApp(P.AppPatterns, app),
                P.ForApp(P.AppWorkflows, app),
                P.ForApp(P.AppRules, app),
                P.ForApp(P.AppSchemas, app));
        }
    }
}
