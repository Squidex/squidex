// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class RolesJsonTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var sut = Roles.CreateDefaults("my-app");

            var roles = sut.SerializeAndDeserialize();

            roles.Should().BeEquivalentTo(sut);
        }
    }
}
