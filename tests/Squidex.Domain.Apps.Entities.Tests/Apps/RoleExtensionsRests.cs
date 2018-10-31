// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class RoleExtensionsRests
    {
        [Fact]
        public void Should_add_common_permission()
        {
            var source = new string[0];
            var result = source.Prefix("my-app");

            Assert.Equal(new[] { "squidex.apps.my-app.common" }, result);
        }

        [Fact]
        public void Should_prefix_permission()
        {
            var source = new[] { "clients.read" };
            var result = source.Prefix("my-app");

            Assert.Equal("squidex.apps.my-app.clients.read", result[1]);
        }
    }
}
