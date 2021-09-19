// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class BackupServiceTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DomainId backupId = DomainId.NewGuid();
        private readonly RefToken actor = RefToken.User("me");
        private readonly BackupService sut;

        public BackupServiceTests()
        {
            sut = new BackupService(grainFactory);
        }

        [Fact]
        public async Task Should_call_grain_if_restoring_backup()
        {
            var grain = A.Fake<IRestoreGrain>();

            A.CallTo(() => grainFactory.GetGrain<IRestoreGrain>(SingleGrain.Id, null))
                .Returns(grain);

            var initiator = RefToken.User("me");

            var restoreUrl = new Uri("http://squidex.io");
            var restoreAppName = "New App";

            await sut.StartRestoreAsync(initiator, restoreUrl, restoreAppName);

            A.CallTo(() => grain.RestoreAsync(restoreUrl, initiator, restoreAppName))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_to_get_restore_status()
        {
            IRestoreJob state = new RestoreJob();

            var grain = A.Fake<IRestoreGrain>();

            A.CallTo(() => grainFactory.GetGrain<IRestoreGrain>(SingleGrain.Id, null))
                .Returns(grain);

            A.CallTo(() => grain.GetStateAsync())
                .Returns(state.AsJ());

            var result = await sut.GetRestoreAsync();

            Assert.Same(state, result);
        }

        [Fact]
        public async Task Should_call_grain_to_start_backup()
        {
            var grain = A.Fake<IBackupGrain>();

            A.CallTo(() => grainFactory.GetGrain<IBackupGrain>(appId.ToString(), null))
                .Returns(grain);

            await sut.StartBackupAsync(appId, actor);

            A.CallTo(() => grain.BackupAsync(actor))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_to_get_backups()
        {
            var state = new List<IBackupJob>
            {
                new BackupJob { Id = backupId }
            };

            var grain = A.Fake<IBackupGrain>();

            A.CallTo(() => grainFactory.GetGrain<IBackupGrain>(appId.ToString(), null))
                .Returns(grain);

            A.CallTo(() => grain.GetStateAsync())
                .Returns(state.AsJ());

            var result = await sut.GetBackupsAsync(appId);

            Assert.Same(state, result);
        }

        [Fact]
        public async Task Should_call_grain_to_get_backup()
        {
            var state = new List<IBackupJob>
            {
                new BackupJob { Id = backupId }
            };

            var grain = A.Fake<IBackupGrain>();

            A.CallTo(() => grainFactory.GetGrain<IBackupGrain>(appId.ToString(), null))
                .Returns(grain);

            A.CallTo(() => grain.GetStateAsync())
                .Returns(state.AsJ());

            var result1 = await sut.GetBackupAsync(appId, backupId);
            var result2 = await sut.GetBackupAsync(appId, DomainId.NewGuid());

            Assert.Same(state[0], result1);
            Assert.Null(result2);
        }

        [Fact]
        public async Task Should_call_grain_to_delete_backup()
        {
            var grain = A.Fake<IBackupGrain>();

            A.CallTo(() => grainFactory.GetGrain<IBackupGrain>(appId.ToString(), null))
                .Returns(grain);

            await sut.DeleteBackupAsync(appId, backupId);

            A.CallTo(() => grain.DeleteAsync(backupId))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_to_clear_backups()
        {
            var grain = A.Fake<IBackupGrain>();

            A.CallTo(() => grainFactory.GetGrain<IBackupGrain>(appId.ToString(), null))
                .Returns(grain);

            await ((IDeleter)sut).DeleteAppAsync(Mocks.App(NamedId.Of(appId, "my-app")), default);

            A.CallTo(() => grain.ClearAsync())
                .MustHaveHappened();
        }
    }
}
