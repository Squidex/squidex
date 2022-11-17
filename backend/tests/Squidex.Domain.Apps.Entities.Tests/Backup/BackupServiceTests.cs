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

public class BackupServiceTests
{
    private readonly TestState<BackupState> stateBackup;
    private readonly TestState<BackupRestoreState> stateRestore;
    private readonly IMessageBus messaging = A.Fake<IMessageBus>();
    private readonly DomainId appId = DomainId.NewGuid();
    private readonly DomainId backupId = DomainId.NewGuid();
    private readonly RefToken actor = RefToken.User("me");
    private readonly BackupService sut;

    public BackupServiceTests()
    {
        stateRestore = new TestState<BackupRestoreState>("Default");
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
        stateRestore.Snapshot = new BackupRestoreState
        {
            Job = new RestoreJob
            {
                Status = JobStatus.Started
            }
        };

        var restoreUrl = new Uri("http://squidex.io");

        await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartRestoreAsync(actor, restoreUrl, null));
    }

    [Fact]
    public async Task Should_throw_exception_when_backup_has_too_many_jobs()
    {
        for (var i = 0; i < 10; i++)
        {
            stateBackup.Snapshot.Jobs.Add(new BackupJob());
        }

        await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartBackupAsync(appId, actor));
    }

    [Fact]
    public async Task Should_throw_exception_when_backup_has_one_running_job()
    {
        for (var i = 0; i < 2; i++)
        {
            stateBackup.Snapshot.Jobs.Add(new BackupJob { Status = JobStatus.Started });
        }

        await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartBackupAsync(appId, actor));
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

        var actual = await sut.GetRestoreAsync();

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

        var actual = await sut.GetBackupsAsync(appId);

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

        var actual = await sut.GetBackupAsync(appId, backupId);

        actual.Should().BeEquivalentTo(job);
    }
}
