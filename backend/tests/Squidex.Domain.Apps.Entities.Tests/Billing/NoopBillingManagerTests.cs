// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Billing
{
    public class NoopBillingManagerTests
    {
        private readonly NoopBillingManager sut = new NoopBillingManager();

        [Fact]
        public void Should_not_have_portal()
        {
            Assert.False(sut.HasPortal);
        }

        [Fact]
        public async Task Should_do_nothing_if_subscribing()
        {
            await sut.SubscribeAsync(null!, null!, null!);
        }

        [Fact]
        public async Task Should_do_nothing_if_subscribing_to_team()
        {
            await sut.SubscribeAsync(null!, default(DomainId), null!);
        }

        [Fact]
        public async Task Should_do_nothing_if_unsubscribing()
        {
            await sut.UnsubscribeAsync(null!, null!);
        }

        [Fact]
        public async Task Should_do_nothing_if_unsubscribing_from_team()
        {
            await sut.UnsubscribeAsync(null!, default(DomainId));
        }

        [Fact]
        public async Task Should_not_return_portal_link()
        {
            var actual = await sut.GetPortalLinkAsync(null!);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_do_nothing_if_checking_for_redirect()
        {
            var actual = await sut.MustRedirectToPortalAsync(null!, null!, null);

            Assert.Null(actual);
        }

        [Fact]
        public async Task Should_do_nothing_if_checking_for_redirect_for_team()
        {
            var actual = await sut.MustRedirectToPortalAsync(null!, default(DomainId), null);

            Assert.Null(actual);
        }
    }
}
