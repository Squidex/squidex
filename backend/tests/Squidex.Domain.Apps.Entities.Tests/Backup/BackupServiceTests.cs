// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Messaging;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class BackupServiceTests
    {
        private readonly TestState<BackupState> stateBackup;
        private readonly TestState<RestoreJob> stateRestore;
        private readonly IMessageBus messaging = A.Fake<IMessageBus>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DomainId backupId = DomainId.NewGuid();
        private readonly RefToken actor = RefToken.User("me");
        private readonly BackupService sut;

        public BackupServiceTests()
        {
            stateRestore = new TestState<RestoreJob>("Default");
            stateBackup = new TestState<BackupState>(appId);

            sut = new BackupService(
                stateRestore.PersistenceFactory,
                stateBackup.PersistenceFactory, messaging);
        }

        [Fact]
        public async Task Should_send_message_to_restore_backup()
        {
            var restoreUrl = new Uri("http://squidex.io");
            var restoreAppName = "New App";

            await sut.StartRestoreAsync(actor, restoreUrl, restoreAppName);

            A.CallTo(() => messaging.PublishAsync(new BackupRestore(actor, restoreUrl, restoreAppName), null, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_message_to_start_backup()
        {
            await sut.StartBackupAsync(appId, actor);

            A.CallTo(() => messaging.PublishAsync(new BackupStart(appId, actor), null, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_message_to_delete_backup()
        {
            await sut.DeleteBackupAsync(appId, backupId);

            A.CallTo(() => messaging.PublishAsync(new BackupDelete(appId, backupId), null, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_message_to_clear_backups()
        {
            await ((IDeleter)sut).DeleteAppAsync(Mocks.App(NamedId.Of(appId, "my-app")), default);

            A.CallTo(() => messaging.PublishAsync(new BackupClear(appId), null, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_when_restore_already_running()
        {
            stateRestore.Value = new RestoreJob
            {
                Status = JobStatus.Started
            };

            var restoreUrl = new Uri("http://squidex.io");

            await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartRestoreAsync(actor, restoreUrl, null));
        }

        [Fact]
        public async Task Should_throw_exception_when_backup_has_too_many_jobs()
        {
            for (var i = 0; i < 10; i++)
            {
                stateBackup.Value.Jobs.Add(new BackupJob());
            }

            await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartBackupAsync(appId, actor));
        }

        [Fact]
        public async Task Should_throw_exception_when_backup_has_one_running_job()
        {
            for (var i = 0; i < 2; i++)
            {
                stateBackup.Value.Jobs.Add(new BackupJob { Status = JobStatus.Started });
            }

            await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartBackupAsync(appId, actor));
        }

        [Fact]
        public async Task Should_get_restore_state_from_store()
        {
            stateRestore.Value = new RestoreJob
            {
                Stopped = SystemClock.Instance.GetCurrentInstant()
            };

            var result = await sut.GetRestoreAsync();

            result.Should().BeEquivalentTo(stateRestore.Value);
        }

        [Fact]
        public async Task Should_get_backups_state_from_store()
        {
            var job = new BackupJob
            {
                Id = backupId,
                Started = SystemClock.Instance.GetCurrentInstant(),
                Stopped = SystemClock.Instance.GetCurrentInstant()
            };

            stateBackup.Value.Jobs.Add(job);

            var result = await sut.GetBackupsAsync(appId);

            result.Should().BeEquivalentTo(stateBackup.Value.Jobs);
        }

        [Fact]
        public async Task Should_get_backup_state_from_store()
        {
            var job = new BackupJob
            {
                Id = backupId,
                Started = SystemClock.Instance.GetCurrentInstant(),
                Stopped = SystemClock.Instance.GetCurrentInstant()
            };

            stateBackup.Value.Jobs.Add(job);

            var result = await sut.GetBackupAsync(appId, backupId);

            result.Should().BeEquivalentTo(job);
        }
    }
}
