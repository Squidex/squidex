// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class RoleExtensionsTests
    {
        [Fact]
        public void Should_add_common_permission()
        {
            var source = Array.Empty<string>();
            var result = source.Prefix("my-app");

            Assert.Equal(new[] { "squidex.apps.my-app.common" }, result);
        }

        [Fact]
        public void Should_not_have_duplicate_permission()
        {
            var source = new[] { "common", "common", "common" };
            var result = source.Prefix("my-app");

            Assert.Single(result);
        }

        [Fact]
        public void Should_prefix_permission()
        {
            var source = new[] { "clients.read" };
            var result = source.Prefix("my-app");

            Assert.Equal("squidex.apps.my-app.clients.read", result[1]);
        }

        [Fact]
        public void Should_remove_app_prefix()
        {
            var source = new PermissionSet("squidex.apps.my-app.clients");
            var result = source.WithoutApp("my-app");

            Assert.Equal("clients", result.First().Id);
        }

        [Fact]
        public void Should_not_remove_app_prefix_when_other_app()
        {
            var source = new PermissionSet("squidex.apps.other-app.clients");
            var result = source.WithoutApp("my-app");

            Assert.Equal("squidex.apps.other-app.clients", result.First().Id);
        }

        [Fact]
        public void Should_set_to_wildcard_when_app_root_permission()
        {
            var source = new PermissionSet("squidex.apps.my-app");
            var result = source.WithoutApp("my-app");

            Assert.Equal(Permission.Any, result.First().Id);
        }

        [Fact]
        public void Should_remove_common_permission()
        {
            var source = new PermissionSet("squidex.apps.my-app.common");
            var result = source.WithoutApp("my-app");

            Assert.Empty(result);
        }
    }
}
