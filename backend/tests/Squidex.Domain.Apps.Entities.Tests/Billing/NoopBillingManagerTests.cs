// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Teams;

namespace Squidex.Domain.Apps.Entities.Billing;

public class NoopBillingManagerTests
{
    private readonly NoopBillingManager sut = new NoopBillingManager();

    [Fact]
    public async Task Should_do_nothing_if_subscribing_to_app()
    {
        await sut.SubscribeAsync(null!, (IAppEntity)null!, null!);
    }

    [Fact]
    public async Task Should_do_nothing_if_subscribing_to_team()
    {
        await sut.SubscribeAsync(null!, (ITeamEntity)null!, null!);
    }

    [Fact]
    public async Task Should_do_nothing_if_unsubscribing_from_app()
    {
        await sut.UnsubscribeAsync(null!, (IAppEntity)null!);
    }

    [Fact]
    public async Task Should_do_nothing_if_unsubscribing_from_team()
    {
        await sut.UnsubscribeAsync(null!, (ITeamEntity)null!);
    }

    [Fact]
    public async Task Should_not_return_portal_link_for_app()
    {
        var actual = await sut.GetPortalLinkAsync(null!, (IAppEntity)null!);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_not_return_portal_link_for_team()
    {
        var actual = await sut.GetPortalLinkAsync(null!, (ITeamEntity)null!);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_not_return_referral_code_for_app()
    {
        var actual = await sut.GetReferralInfoAsync(null!, (IAppEntity)null!);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_not_return_referral_code_for_team()
    {
        var actual = await sut.GetReferralInfoAsync(null!, (ITeamEntity)null!);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_do_nothing_if_checking_for_redirect_for_app()
    {
        var actual = await sut.MustRedirectToPortalAsync(null!, (IAppEntity)null!, null);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_do_nothing_if_checking_for_redirect_for_team()
    {
        var actual = await sut.MustRedirectToPortalAsync(null!, (ITeamEntity)null!, null);

        Assert.Null(actual);
    }
}
