// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using tusdotnet.Interfaces;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetCleanupGrainTests
    {
        private readonly ITusExpirationStore expirationStore = A.Fake<ITusExpirationStore>();
        private readonly AssetCleanupGrain sut;

        public AssetCleanupGrainTests()
        {
            sut = new AssetCleanupGrain(expirationStore);
        }

        [Fact]
        public async Task Should_do_nothing_on_activate()
        {
            await sut.ActivateAsync();
        }

        [Fact]
        public async Task Should_call_expiration_store_when_reminder_invoked()
        {
            await sut.ReceiveReminder("Reminder", default);

            A.CallTo(() => expirationStore.RemoveExpiredFilesAsync(default))
                .MustHaveHappened();
        }
    }
}
