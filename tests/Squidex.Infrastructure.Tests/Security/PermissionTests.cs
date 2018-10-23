// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Squidex.Infrastructure.Security
{
    public class PermissionTests
    {
        [Fact]
        public void Should_generate_permissions()
        {
            var sut = new Permission("app.contents", "App Contents");

            Assert.Equal("app.contents", sut.ToString());
            Assert.Equal("app.contents", sut.Id);
            Assert.Equal("App Contents", sut.Description);
        }

        [Fact]
        public void Should_return_true_if_given_and_requested_permission_have_wildcards()
        {
            var g = new Permission("app.*");
            var r = new Permission("app.*");

            Assert.True(g.GivesPermissionTo(r));
        }

        [Fact]
        public void Should_return_true_if_given_permission_equals_requested_permission()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app.contents");

            Assert.True(g.GivesPermissionTo(r));
        }

        [Fact]
        public void Should_return_true_if_given_permission_is_parent_of_requested_permission()
        {
            var g = new Permission("app");
            var r = new Permission("app.contents");

            Assert.True(g.GivesPermissionTo(r));
        }

        [Fact]
        public void Should_return_true_if_given_permission_has_wildcard_for_requested_permission()
        {
            var g = new Permission("app.*");
            var r = new Permission("app.contents");

            Assert.True(g.GivesPermissionTo(r));
        }

        [Fact]
        public void Should_return_false_if_given_permission_not_equals_requested_permission()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app.assets");

            Assert.False(g.GivesPermissionTo(r));
        }

        [Fact]
        public void Should_return_false_if_given_permission_is_child_of_requested_permission()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app");

            Assert.False(g.GivesPermissionTo(r));
        }

        [Fact]
        public void Should_return_false_if_given_permission_has_no_wildcard_but_requested_has()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app.*");

            Assert.False(g.GivesPermissionTo(r));
        }

        [Fact]
        public void Should_return_false_if_given_requested_permission_is_null()
        {
            var g = new Permission("app.contents");

            Assert.False(g.GivesPermissionTo(null));
        }

        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            object permission1a = new Permission("app.1");
            object permission1b = new Permission("app.1");
            object permission2a = new Permission("app.2");

            Assert.True(permission1a.Equals(permission1b));

            Assert.False(permission1a.Equals(permission2a));
            Assert.False(permission1b.Equals(permission2a));
        }

        [Fact]
        public void Should_provide_correct_hash_codes()
        {
            var permission1a = new Permission("app.1");
            var permission1b = new Permission("app.1");
            var permission2a = new Permission("app.2");

            Assert.Equal(permission1a.GetHashCode(), permission1b.GetHashCode());

            Assert.NotEqual(permission1a.GetHashCode(), permission2a.GetHashCode());
            Assert.NotEqual(permission1b.GetHashCode(), permission2a.GetHashCode());
        }

        [Fact]
        public void Should_sort_by_name()
        {
            var source = new List<Permission>
            {
                new Permission("c"),
                new Permission("b"),
                new Permission("a")
            };

            var sorted = source.OrderBy(x => x).ToList();

            Assert.Equal(new List<Permission> { source[2], source[1], source[0] }, sorted);
        }
    }
}
