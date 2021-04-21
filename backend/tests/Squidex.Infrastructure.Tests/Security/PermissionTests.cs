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
        public void Should_generate_permission()
        {
            var sut = new Permission("app.contents");

            Assert.Equal("app.contents", sut.ToString());
            Assert.Equal("app.contents", sut.Id);
        }

        [Fact]
        public void Should_allow_and_include_if_permissions_are_equal()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app.contents");

            Assert.True(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_not_allow_and_include_if_permissions_are_not_equal()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app.assets");

            Assert.False(g.Allows(r));
            Assert.False(g.Includes(r));
        }

        [Fact]
        public void Should_allow_and_include_if_permissions_have_same_wildcards()
        {
            var g = new Permission("app.*");
            var r = new Permission("app.*");

            Assert.True(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_allow_and_include_if_given_is_parent_of_requested()
        {
            var g = new Permission("app");
            var r = new Permission("app.contents");

            Assert.True(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_not_allow_but_include_if_requested_is_parent_of_given()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app");

            Assert.False(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_allow_and_include_if_given_is_wildcard_of_requested()
        {
            var g = new Permission("app.*");
            var r = new Permission("app.contents");

            Assert.True(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_not_allow_but_include_if_given_is_wildcard_of_requested()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app.*");

            Assert.False(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_allow_and_include_if_given_has_alternatives_of_requested()
        {
            var g = new Permission("app.contents|schemas");
            var r = new Permission("app.contents");

            Assert.True(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_allow_and_include_if_given_has_not_excluded_requested()
        {
            var g = new Permission("app.^schemas");
            var r = new Permission("app.contents");

            Assert.True(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_allow_and_include_if_requested_has_not_excluded_given()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app.^schemas");

            Assert.True(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_not_allow_and_include_if_given_has_excluded_requested()
        {
            var g = new Permission("app.^contents");
            var r = new Permission("app.contents");

            Assert.False(g.Allows(r));
            Assert.False(g.Includes(r));
        }

        [Fact]
        public void Should_not_allow_and_include_if_given_and_requested_have_same_exclusion()
        {
            var g = new Permission("app.^contents");
            var r = new Permission("app.^contents");

            Assert.True(g.Allows(r));
            Assert.True(g.Includes(r));
        }

        [Fact]
        public void Should_allow_and_include_if_requested_is_has_alternatives_of_given()
        {
            var g = new Permission("app.contents");
            var r = new Permission("app.contents|schemas");

            Assert.True(g.Allows(r));
            Assert.True(g.Includes(r));
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

        [Theory]
        [InlineData("permission")]
        [InlineData("permission...")]
        [InlineData("permission.||..")]
        public void Should_parse_invalid_permissions(string source)
        {
            var permission = new Permission(source);

            permission.Allows(new Permission(Permission.Any));
        }
    }
}
