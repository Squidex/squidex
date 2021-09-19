// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Log;
using Xunit;

namespace Squidex.Infrastructure.Migrations
{
    public class MigratorTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IMigrationStatus status = A.Fake<IMigrationStatus>();
        private readonly IMigrationPath path = A.Fake<IMigrationPath>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly List<(int From, int To, IMigration Migration)> migrations = new List<(int From, int To, IMigration Migration)>();

        public sealed class InMemoryStatus : IMigrationStatus
        {
            private readonly object lockObject = new object();
            private int version;
            private bool isLocked;

            public Task<int> GetVersionAsync(
                CancellationToken ct = default)
            {
                return Task.FromResult(version);
            }

            public Task<bool> TryLockAsync(
                CancellationToken ct = default)
            {
                var lockAcquired = false;

                lock (lockObject)
                {
                    if (!isLocked)
                    {
                        isLocked = true;

                        lockAcquired = true;
                    }
                }

                return Task.FromResult(lockAcquired);
            }

            public Task CompleteAsync(int newVersion,
                CancellationToken ct = default)
            {
                lock (lockObject)
                {
                    version = newVersion;
                }

                return Task.CompletedTask;
            }

            public Task UnlockAsync(
                CancellationToken ct = default)
            {
                lock (lockObject)
                {
                    isLocked = false;
                }

                return Task.CompletedTask;
            }
        }

        public MigratorTests()
        {
            ct = cts.Token;

            A.CallTo(() => path.GetNext(A<int>._))
                .ReturnsLazily((int version) =>
                {
                    var selected = migrations.Where(x => x.From == version).ToList();

                    if (selected.Count == 0)
                    {
                        return (0, null);
                    }

                    var newVersion = selected.Max(x => x.To);

                    return (newVersion, migrations.Select(x => x.Migration));
                });

            A.CallTo(() => status.GetVersionAsync(ct))
                .Returns(0);

            A.CallTo(() => status.TryLockAsync(ct))
                .Returns(true);
        }

        [Fact]
        public async Task Should_migrate_in_one_step()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(0, 2);
            var migrator_2_3 = BuildMigration(0, 3);

            var sut = new Migrator(status, path, log);

            await sut.MigrateAsync(ct);

            A.CallTo(() => migrator_0_1.UpdateAsync(ct))
                .MustHaveHappened();

            A.CallTo(() => migrator_1_2.UpdateAsync(ct))
                .MustHaveHappened();

            A.CallTo(() => migrator_2_3.UpdateAsync(ct))
                .MustHaveHappened();

            A.CallTo(() => status.CompleteAsync(1, ct))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(2, ct))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(3, ct))
                .MustHaveHappened();

            A.CallTo(() => status.UnlockAsync(default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_migrate_step_by_step()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);
            var migrator_2_3 = BuildMigration(2, 3);

            var sut = new Migrator(status, path, log);

            await sut.MigrateAsync(ct);

            A.CallTo(() => migrator_0_1.UpdateAsync(ct))
                .MustHaveHappened();

            A.CallTo(() => migrator_1_2.UpdateAsync(ct))
                .MustHaveHappened();

            A.CallTo(() => migrator_2_3.UpdateAsync(ct))
                .MustHaveHappened();

            A.CallTo(() => status.CompleteAsync(1, ct))
                .MustHaveHappened();

            A.CallTo(() => status.CompleteAsync(2, ct))
                .MustHaveHappened();

            A.CallTo(() => status.CompleteAsync(3, ct))
                .MustHaveHappened();

            A.CallTo(() => status.UnlockAsync(A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_unlock_if_migration_failed()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);
            var migrator_2_3 = BuildMigration(2, 3);

            var sut = new Migrator(status, path, log);

            A.CallTo(() => migrator_1_2.UpdateAsync(ct))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<MigrationFailedException>(() => sut.MigrateAsync(ct));

            A.CallTo(() => migrator_0_1.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => migrator_1_2.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => migrator_2_3.UpdateAsync(A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(1, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(2, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(3, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => status.UnlockAsync(default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_log_exception_if_migration_failed()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);

            var ex = new InvalidOperationException();

            A.CallTo(() => migrator_0_1.UpdateAsync(ct))
                .Throws(ex);

            var sut = new Migrator(status, path, log);

            await Assert.ThrowsAsync<MigrationFailedException>(() => sut.MigrateAsync(ct));

            A.CallTo(() => log.Log(SemanticLogLevel.Fatal, ex, A<LogFormatter>._!))
                .MustHaveHappened();

            A.CallTo(() => migrator_1_2.UpdateAsync(ct))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_prevent_multiple_updates()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(0, 2);

            var sut = new Migrator(new InMemoryStatus(), path, log) { LockWaitMs = 2 };

            await Task.WhenAll(Enumerable.Repeat(0, 10).Select(x => Task.Run(() => sut.MigrateAsync(ct), ct)));

            A.CallTo(() => migrator_0_1.UpdateAsync(ct))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => migrator_1_2.UpdateAsync(ct))
                .MustHaveHappenedOnceExactly();
        }

        private IMigration BuildMigration(int fromVersion, int toVersion)
        {
            var migration = A.Fake<IMigration>();

            migrations.Add((fromVersion, toVersion, migration));

            return migration;
        }
    }
}
