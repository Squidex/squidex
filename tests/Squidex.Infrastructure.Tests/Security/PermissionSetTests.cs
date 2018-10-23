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
        }

        [Fact]
        public void Should_return_true_if_any_permission_gives_permission_to_request()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.True(sut.GivesPermissionTo(new Permission("app.contents")));
        }

        [Fact]
        public void Should_return_false_if_none_permission_gives_permission_to_request()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.False(sut.GivesPermissionTo(new Permission("app.schemas")));
        }

        [Fact]
        public void Should_return_false_if_permission_to_request_is_null()
        {
            var sut = new PermissionSet(
                new Permission("app.contents"),
                new Permission("app.assets"));

            Assert.False(sut.GivesPermissionTo(null));
        }
    }
}
