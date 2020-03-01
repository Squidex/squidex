// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Web
{
    public class ApiCostsAttributeTests
    {
        [Fact]
        public void Should_assign_costs()
        {
            var sut = new ApiCostsAttribute(10.5);

            Assert.Equal(10.5, sut.Costs);
        }
    }
}
