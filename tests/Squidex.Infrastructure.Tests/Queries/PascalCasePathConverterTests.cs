// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.Queries
{
    public class PascalCasePathConverterTests
    {
        [Fact]
        public void Should_convert_property()
        {
            var source = FilterBuilder.Eq("property", 1);
            var result = PascalCasePathConverter.Transform(source);

            Assert.Equal("Property == 1", result.ToString());
        }

        [Fact]
        public void Should_convert_properties()
        {
            var source = FilterBuilder.Eq("root.child", 1);
            var result = PascalCasePathConverter.Transform(source);

            Assert.Equal("Root.Child == 1", result.ToString());
        }
    }
}
