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
            var role = new Role("Owner");

            Assert.True(role.IsDefault);
        }

        [Fact]
        public void Should_not_be_default_role()
        {
            var role = new Role("Custom");

            Assert.False(role.IsDefault);
        }

        [Fact]
        public void Should_add_common_permission()
        {
            var role = new Role("Name");

            var result = role.ForApp("my-app").Permissions.ToIds();

            Assert.Equal(new[] { "squidex.apps.my-app.common" }, result);
        }

        [Fact]
        public void Should_not_have_duplicate_permission()
        {
            var role = new Role("Name", "common", "common", "common");

            var result = role.ForApp("my-app").Permissions.ToIds();

            Assert.Single(result);
        }

        [Fact]
        public void Should_ForApp_permission()
        {
            var role = new Role("Name", "clients.read");

            var result = role.ForApp("my-app").Permissions.ToIds();

            Assert.Equal("squidex.apps.my-app.clients.read", result.ElementAt(1));
        }

        [Fact]
        public void Should_check_for_name()
        {
            var role = new Role("Custom");

            Assert.True(role.Equals("Custom"));
        }

        [Fact]
        public void Should_check_for_null_name()
        {
            var role = new Role("Custom");

            Assert.False(role.Equals(null));
            Assert.False(role.Equals("Other"));
        }
    }
}
