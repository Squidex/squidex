// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
        public async Task Should_do_nothing_if_subscribing()
        {
            await sut.SubscribeAsync(null!, null!, null, null);
        }

        [Fact]
        public async Task Should_do_nothing_if_unsubscribing()
        {
            await sut.SubscribeAsync(null!, null!, null, null);
        }

        [Fact]
        public async Task Should_not_return_portal_link()
        {
            var result = await sut.GetPortalLinkAsync(null!);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_do_nothing_if_checking_for_redirect()
        {
            var result = await sut.MustRedirectToPortalAsync(null!, null!, null, null);

            Assert.Null(result);
        }
    }
}
