// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Squidex.Infrastructure.Security
{
    public sealed class PermissionSetTests
    {
        [Fact]
        public void Should_provide_collection_features()
        {
            var source = new List<Permission>
            {
                new Permission("c"),
                new Permission("b"),
                new Permission("a")
            };

            var sut = new PermissionSet(source);

            Assert.Equal(sut.ToList(), source);
            Assert.Equal(((IEnumerable)sut).OfType<Permission>().ToList(), source);

            Assert.Equal(3, source.Count);

            Assert.Equal("c;b;a", sut.ToString());
        }

        [Fact]
        public void Should_return_true_if_any_permission_gives_permission_to_requested()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.True(sut.Allows(new Permission("app.contents")));
        }

        [Fact]
        public void Should_return_true_if_any_permission_includes_parent_given()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.True(sut.Includes(new Permission("app")));
        }

        [Fact]
        public void Should_return_true_if_any_permission_includes_child_given()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.True(sut.Includes(new Permission("app.contents.read")));
        }

        [Fact]
        public void Should_return_false_if_none_permission_gives_permission_to_requested()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.False(sut.Allows(new Permission("app.schemas")));
        }

        [Fact]
        public void Should_return_false_if_none_permission_includes_given()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.False(sut.Includes(new Permission("other")));
        }

        [Fact]
        public void Should_return_false_if_permission_to_request_is_null()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.False(sut.Allows(null));
        }

        [Fact]
        public void Should_return_false_if_permission_to_include_is_null()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.False(sut.Includes(null));
        }
    }
}
