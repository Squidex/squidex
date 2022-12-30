// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Backup;

public class BackupServiceTests : GivenContext
{
    private readonly TestState<BackupState> stateBackup;
    private readonly TestState<BackupRestoreState> stateRestore;
    private readonly IMessageBus messaging = A.Fake<IMessageBus>();
    private readonly DomainId backupId = DomainId.NewGuid();
    private readonly BackupService sut;

    public BackupServiceTests()
    {
        stateRestore = new TestState<BackupRestoreState>("Default");
        stateBackup = new TestState<BackupState>(AppId.Id);

        sut = new BackupService(
            stateRestore.PersistenceFactory,
            stateBackup.PersistenceFactory, messaging);
    }

    [Fact]
    public async Task Should_send_message_to_restore_backup()
    {
        var restoreUrl = new Uri("http://squidex.io");
        var restoreAppName = "New App";

        await sut.StartRestoreAsync(User, restoreUrl, restoreAppName, CancellationToken);

        A.CallTo(() => messaging.PublishAsync(new BackupRestore(User, restoreUrl, restoreAppName), null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_send_message_to_start_backup()
    {
        await sut.StartBackupAsync(AppId.Id, User, CancellationToken);

        A.CallTo(() => messaging.PublishAsync(new BackupStart(AppId.Id, User), null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_send_message_to_delete_backup()
    {
        await sut.DeleteBackupAsync(AppId.Id, backupId, CancellationToken);

        A.CallTo(() => messaging.PublishAsync(new BackupDelete(AppId.Id, backupId), null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_send_message_to_clear_backups()
    {
        await ((IDeleter)sut).DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => messaging.PublishAsync(new BackupClear(AppId.Id), null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_when_restore_already_running()
    {
        stateRestore.Snapshot = new BackupRestoreState
        {
            Job = new RestoreJob
            {
                Status = JobStatus.Started
            }
        };

        var restoreUrl = new Uri("http://squidex.io");

        await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartRestoreAsync(User, restoreUrl, null, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_when_backup_has_too_many_jobs()
    {
        for (var i = 0; i < 10; i++)
        {
            stateBackup.Snapshot.Jobs.Add(new BackupJob());
        }

        await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartBackupAsync(AppId.Id, User, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_when_backup_has_one_running_job()
    {
        for (var i = 0; i < 2; i++)
        {
            stateBackup.Snapshot.Jobs.Add(new BackupJob { Status = JobStatus.Started });
        }

        await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartBackupAsync(AppId.Id, User, CancellationToken));
    }

    [Fact]
    public async Task Should_get_restore_state_from_store()
    {
        stateRestore.Snapshot = new BackupRestoreState
        {
            Job = new RestoreJob
            {
                Stopped = SystemClock.Instance.GetCurrentInstant()
            }
        };

        var actual = await sut.GetRestoreAsync(CancellationToken);

        actual.Should().BeEquivalentTo(stateRestore.Snapshot.Job);
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

        stateBackup.Snapshot.Jobs.Add(job);

        var actual = await sut.GetBackupsAsync(AppId.Id, CancellationToken);

        actual.Should().BeEquivalentTo(stateBackup.Snapshot.Jobs);
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

        stateBackup.Snapshot.Jobs.Add(job);

        var actual = await sut.GetBackupAsync(AppId.Id, backupId, CancellationToken);

        actual.Should().BeEquivalentTo(job);
    }
}
