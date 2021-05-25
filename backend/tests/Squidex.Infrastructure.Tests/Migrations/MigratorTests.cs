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
        private readonly IMigrationStatus status = A.Fake<IMigrationStatus>();
        private readonly IMigrationPath path = A.Fake<IMigrationPath>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly List<(int From, int To, IMigration Migration)> migrations = new List<(int From, int To, IMigration Migration)>();

        public sealed class InMemoryStatus : IMigrationStatus
        {
            private readonly object lockObject = new object();
            private int version;
            private bool isLocked;

            public Task<int> GetVersionAsync()
            {
                return Task.FromResult(version);
            }

            public Task<bool> TryLockAsync()
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

            public Task CompleteAsync(int newVersion)
            {
                lock (lockObject)
                {
                    version = newVersion;
                }

                return Task.CompletedTask;
            }

            public Task UnlockAsync()
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

            A.CallTo(() => status.GetVersionAsync()).Returns(0);
            A.CallTo(() => status.TryLockAsync()).Returns(true);
        }

        [Fact]
        public async Task Should_migrate_in_one_step()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(0, 2);
            var migrator_2_3 = BuildMigration(0, 3);

            var sut = new Migrator(status, path, log);

            await sut.MigrateAsync();

            A.CallTo(() => migrator_0_1.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => migrator_1_2.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => migrator_2_3.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => status.CompleteAsync(1))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(2))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(3))
                .MustHaveHappened();

            A.CallTo(() => status.UnlockAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_migrate_step_by_step()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);
            var migrator_2_3 = BuildMigration(2, 3);

            var sut = new Migrator(status, path, log);

            await sut.MigrateAsync();

            A.CallTo(() => migrator_0_1.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => migrator_1_2.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => migrator_2_3.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => status.CompleteAsync(1))
                .MustHaveHappened();

            A.CallTo(() => status.CompleteAsync(2))
                .MustHaveHappened();

            A.CallTo(() => status.CompleteAsync(3))
                .MustHaveHappened();

            A.CallTo(() => status.UnlockAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_unlock_if_migration_failed()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);
            var migrator_2_3 = BuildMigration(2, 3);

            var sut = new Migrator(status, path, log);

            A.CallTo(() => migrator_1_2.UpdateAsync(A<CancellationToken>._)).Throws(new ArgumentException());

            await Assert.ThrowsAsync<MigrationFailedException>(() => sut.MigrateAsync());

            A.CallTo(() => migrator_0_1.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => migrator_1_2.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappened();

            A.CallTo(() => migrator_2_3.UpdateAsync(A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(1))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(2))
                .MustNotHaveHappened();

            A.CallTo(() => status.CompleteAsync(3))
                .MustNotHaveHappened();

            A.CallTo(() => status.UnlockAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_log_exception_if_migration_failed()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);

            var ex = new InvalidOperationException();

            A.CallTo(() => migrator_0_1.UpdateAsync(A<CancellationToken>._))
                .Throws(ex);

            var sut = new Migrator(status, path, log);

            await Assert.ThrowsAsync<MigrationFailedException>(() => sut.MigrateAsync());

            A.CallTo(() => log.Log(SemanticLogLevel.Fatal, ex, A<LogFormatter>._!))
                .MustHaveHappened();

            A.CallTo(() => migrator_1_2.UpdateAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_prevent_multiple_updates()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(0, 2);

            var sut = new Migrator(new InMemoryStatus(), path, log) { LockWaitMs = 2 };

            await Task.WhenAll(Enumerable.Repeat(0, 10).Select(x => Task.Run(() => sut.MigrateAsync())));

            A.CallTo(() => migrator_0_1.UpdateAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => migrator_1_2.UpdateAsync(A<CancellationToken>._))
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
