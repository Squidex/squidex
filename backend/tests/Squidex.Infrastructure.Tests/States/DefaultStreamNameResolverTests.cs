// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Xunit;

namespace Squidex.Infrastructure.States
{
    public class DefaultStreamNameResolverTests
    {
        private readonly DefaultStreamNameResolver sut = new DefaultStreamNameResolver();

        private sealed class MyUser
        {
        }

        private sealed class MyUserDomainObject
        {
        }

        private readonly string id = Guid.NewGuid().ToString();

        [Fact]
        public void Should_calculate_name()
        {
            var name = sut.GetStreamName(typeof(MyUser), id);

            Assert.Equal($"myUser-{id}", name);
        }

        [Fact]
        public void Should_calculate_name_and_remove_suffix()
        {
            var name = sut.GetStreamName(typeof(MyUserDomainObject), id);

            Assert.Equal($"myUser-{id}", name);
        }
    }
}
