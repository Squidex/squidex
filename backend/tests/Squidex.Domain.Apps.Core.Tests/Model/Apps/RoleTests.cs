// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class RoleTests
    {
        [Fact]
        public void Should_be_default_role()
        {
            var role = Role.Create("Owner");

            Assert.True(role.IsDefault);
        }

        [Fact]
        public void Should_not_be_default_role()
        {
            var role = Role.Create("Custom");

            Assert.False(role.IsDefault);
        }

        [Fact]
        public void Should_not_add_common_permission()
        {
            var role = Role.Create("Name");

            var result = role.ForApp("my-app").Permissions.ToIds();

            Assert.Empty(result);
        }

        [Fact]
        public void Should_not_have_duplicate_permission()
        {
            var role = Role.WithPermissions("Name", "common", "common", "common");

            var result = role.ForApp("my-app").Permissions.ToIds();

            Assert.Single(result);
        }

        [Fact]
        public void Should_append_app_prefix_to_permission()
        {
            var role = Role.WithPermissions("Name", "clients.read");

            var result = role.ForApp("my-app").Permissions.ToIds();

            Assert.Equal("squidex.apps.my-app.clients.read", result.ElementAt(0));
        }

        [Fact]
        public void Should_check_for_name()
        {
            var role = Role.WithPermissions("Custom");

            Assert.True(role.Equals("Custom"));
        }

        [Fact]
        public void Should_check_for_null_name()
        {
            var role = Role.WithPermissions("Custom");

            Assert.False(role.Equals((string)null!));
            Assert.False(role.Equals("Other"));
        }
    }
}
