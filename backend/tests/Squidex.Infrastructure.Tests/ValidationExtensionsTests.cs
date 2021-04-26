// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Infrastructure
{
    public class ValidationExtensionsTests
    {
        [Fact]
        public void Should_return_true_if_is_between()
        {
            Assert.True(1.IsBetween(0, 2));
        }

        [Fact]
        public void Should_return_false_if_is_not_between()
        {
            Assert.False(1.IsBetween(2, 3));
        }

        [Fact]
        public void Should_return_true_if_is_valid_regex()
        {
            const string regex = "[a-z]*";

            Assert.True(regex.IsValidRegex());
        }

        [Fact]
        public void Should_return_true_if_is_not_a_valid_regex()
        {
            const string regex = "([a-z]*";

            Assert.False(regex.IsValidRegex());
        }

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