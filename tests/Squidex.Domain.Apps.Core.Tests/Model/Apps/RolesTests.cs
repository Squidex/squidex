// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class RolesTests
    {
        [Fact]
        public void Should_create_defaults()
        {
            var sut = Roles.CreateDefaults("my-app");

            Assert.Equal(4, sut.Count);

            foreach (var role in sut)
            {
                foreach (var permission in role.Value.Permissions)
                {
                    Assert.StartsWith("squidex.apps.my-app", permission.Id);
                }
            }
        }
    }
}
