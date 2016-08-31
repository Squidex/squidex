// ==========================================================================
//  EnumExtensionsTest.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using Xunit;

namespace PinkParrot.Infrastructure
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