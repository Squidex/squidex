// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using tusdotnet.Interfaces;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetCleanupProcessTests
{
    private readonly ITusExpirationStore expirationStore = A.Fake<ITusExpirationStore>();
    private readonly AssetCleanupProcess sut;

    public AssetCleanupProcessTests()
    {
        sut = new AssetCleanupProcess(expirationStore);
    }

    [Fact]
    public async Task Should_stop_when_start_not_called()
    {
        await sut.StopAsync(default);
    }

    [Fact]
    public async Task Should_call_expiration_store_when_reminder_invoked()
    {
        await sut.CleanupAsync(default);

        A.CallTo(() => expirationStore.RemoveExpiredFilesAsync(default))
            .MustHaveHappened();
    }
}
