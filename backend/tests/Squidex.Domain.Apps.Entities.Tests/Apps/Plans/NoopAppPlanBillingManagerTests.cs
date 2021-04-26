// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
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
        public async Task Should_do_nothing_if_changing_plan()
        {
            await sut.ChangePlanAsync(null!, null!, null, null);
        }

        [Fact]
        public async Task Should_not_return_portal_link()
        {
            Assert.Equal(string.Empty, await sut.GetPortalLinkAsync(null!));
        }
    }
}
