// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class RoleExtensionTests
    {
        [Fact]
        public void Should_convert_from_client_permission_to_app_permission()
        {
            Assert.Equal(AppPermission.Developer, AppClientPermission.Developer.ToAppPermission());
            Assert.Equal(AppPermission.Editor, AppClientPermission.Editor.ToAppPermission());
            Assert.Equal(AppPermission.Reader, AppClientPermission.Reader.ToAppPermission());
        }

        [Fact]
        public void Should_throw_when_converting_from_invalid_client_permission()
        {
            Assert.Throws<ArgumentException>(() => ((AppClientPermission)10).ToAppPermission());
        }

        [Fact]
        public void Should_convert_from_contributor_permission_to_app_permission()
        {
            Assert.Equal(AppPermission.Developer, AppContributorPermission.Developer.ToAppPermission());
            Assert.Equal(AppPermission.Editor, AppContributorPermission.Editor.ToAppPermission());
            Assert.Equal(AppPermission.Owner, AppContributorPermission.Owner.ToAppPermission());
        }

        [Fact]
        public void Should_throw_when_converting_from_invalid_contributor_permission()
        {
            Assert.Throws<ArgumentException>(() => ((AppContributorPermission)10).ToAppPermission());
        }
    }
}
