// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.Security;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Apps
{
    public class RolesTests
    {
        private readonly Roles roles_0;
        private readonly string firstRole = "Role1";
        private readonly string role = "Role2";

        public RolesTests()
        {
            roles_0 = Roles.Empty.Add(firstRole);
        }

        [Fact]
        public void Should_add_role()
        {
            var roles_1 = roles_0.Add(role);

            roles_1[role].Should().BeEquivalentTo(new Role(role, PermissionSet.Empty));
        }

        [Fact]
        public void Should_throw_exception_if_add_role_with_same_name()
        {
            var roles_1 = roles_0.Add(role);

            Assert.Throws<ArgumentException>(() => roles_1.Add(role));
        }

        [Fact]
        public void Should_update_role()
        {
            var roles_1 = roles_0.Update(firstRole, "P1", "P2");

            roles_1[firstRole].Should().BeEquivalentTo(new Role(firstRole, new PermissionSet("P1", "P2")));
        }

        [Fact]
        public void Should_return_same_roles_if_role_not_found()
        {
            var roles_1 = roles_0.Update(role, "P1", "P2");

            Assert.Same(roles_0, roles_1);
        }

        [Fact]
        public void Should_remove_role()
        {
            var roles_1 = roles_0.Remove(firstRole);

            Assert.Empty(roles_1);
        }

        [Fact]
        public void Should_do_nothing_if_remove_role_not_found()
        {
            var roles_1 = roles_0.Remove(role);

            Assert.NotEmpty(roles_1);
        }

        [Fact]
        public void Should_create_defaults()
        {
            var sut = Roles.CreateDefaults("my-app");

            Assert.Equal(4, sut.Count);

            foreach (var sutRole in sut)
            {
                foreach (var permission in sutRole.Value.Permissions)
                {
                    Assert.StartsWith("squidex.apps.my-app", permission.Id);
                }
            }
        }
    }
}
