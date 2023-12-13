// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Billing;

public class NoopBillingManagerTests : GivenContext
{
    private readonly NoopBillingManager sut = new NoopBillingManager();

    [Fact]
    public async Task Should_do_nothing_if_subscribing_to_app()
    {
        await sut.SubscribeAsync(User.Identifier, App, "free", CancellationToken);
    }

    [Fact]
    public async Task Should_do_nothing_if_subscribing_to_team()
    {
        await sut.SubscribeAsync(User.Identifier, Team, "free", CancellationToken);
    }

    [Fact]
    public async Task Should_do_nothing_if_unsubscribing_from_app()
    {
        await sut.UnsubscribeAsync(User.Identifier, App, CancellationToken);
    }

    [Fact]
    public async Task Should_do_nothing_if_unsubscribing_from_team()
    {
        await sut.UnsubscribeAsync(User.Identifier, Team, CancellationToken);
    }

    [Fact]
    public async Task Should_not_return_portal_link_for_app()
    {
        var actual = await sut.GetPortalLinkAsync(User.Identifier, App, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_not_return_portal_link_for_team()
    {
        var actual = await sut.GetPortalLinkAsync(User.Identifier, Team, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_not_return_referral_code_for_app()
    {
        var actual = await sut.GetReferralInfoAsync(User.Identifier, App, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_not_return_referral_code_for_team()
    {
        var actual = await sut.GetReferralInfoAsync(User.Identifier, Team, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_do_nothing_if_checking_for_redirect_for_app()
    {
        var actual = await sut.MustRedirectToPortalAsync(User.Identifier, App, "free", CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_do_nothing_if_checking_for_redirect_for_team()
    {
        var actual = await sut.MustRedirectToPortalAsync(User.Identifier, Team, "free", CancellationToken);

        Assert.Null(actual);
    }
}
