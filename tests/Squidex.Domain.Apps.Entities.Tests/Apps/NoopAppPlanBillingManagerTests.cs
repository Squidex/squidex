// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Services.Implementations;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class NoopAppPlanBillingManagerTests
    {
        private readonly NoopAppPlanBillingManager sut = new NoopAppPlanBillingManager();

        [Fact]
        public void Should_not_have_portal()
        {
            Assert.False(sut.HasPortal);
        }

        [Fact]
        public async Task Should_do_nothing_when_changing_plan()
        {
            await sut.ChangePlanAsync(null, Guid.Empty, null, null);
        }

        [Fact]
        public async Task Should_not_return_portal_link()
        {
            Assert.Equal(string.Empty, await sut.GetPortalLinkAsync(null));
        }
    }
}
