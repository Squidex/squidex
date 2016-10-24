// ==========================================================================
//  EnumExtensionsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Xunit;

namespace Squidex.Infrastructure
{
    public sealed class EnumExtensionsTests
    {
        [Fact]
        public void Should_return_true_if_enum_is_valid()
        {
            Assert.True(DateTimeKind.Local.IsEnumValue());
        }

        [Fact]
        public void Should_return_false_if_enum_is_not_valid()
        {
            Assert.False(((DateTimeKind)13).IsEnumValue());
            Assert.False(123.IsEnumValue());
        }
    }
}