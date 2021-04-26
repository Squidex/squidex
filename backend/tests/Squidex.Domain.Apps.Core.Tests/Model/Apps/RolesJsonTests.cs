// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class RolesJsonTests
    {
        [Fact]
        public void Should_deserialize_from_old_role_format()
        {
            var source = new Dictionary<string, string[]>
            {
                ["Custom"] = new[]
                {
                    "Permission1",
                    "Permission2"
                }
            };

            var expected =
                Roles.Empty
                    .Add("Custom")
                    .Update("Custom",
                        new PermissionSet(
                            "Permission1",
                            "Permission2"));

            var roles = source.SerializeAndDeserialize<Roles>();

            roles.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var sut =
                Roles.Empty
                    .Add("Custom")
                    .Update("Custom",
                        new PermissionSet(
                            "Permission1",
                            "Permission2"),
                        JsonValue.Object()
                            .Add("Property1", true)
                            .Add("Property2", true));

            var roles = sut.SerializeAndDeserialize();

            roles.Should().BeEquivalentTo(sut);
        }

        [Fact]
        public void Should_serialize_and_deserialize_empty()
        {
            var sut = Roles.Empty;

            var roles = sut.SerializeAndDeserialize();

            Assert.Same(Roles.Empty, roles);
        }
    }
}
