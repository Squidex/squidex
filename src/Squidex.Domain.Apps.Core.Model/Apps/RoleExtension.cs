// ==========================================================================
//  RoleExtension.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public static class RoleExtension
    {
        public static AppPermission ToAppPermission(this AppClientPermission clientPermission)
        {
            Guard.Enum(clientPermission, nameof(clientPermission));

            return (AppPermission)Enum.Parse(typeof(AppPermission), clientPermission.ToString());
        }

        public static AppPermission ToAppPermission(this AppContributorPermission contributorPermission)
        {
            Guard.Enum(contributorPermission, nameof(contributorPermission));

            return (AppPermission)Enum.Parse(typeof(AppPermission), contributorPermission.ToString());
        }
    }
}
