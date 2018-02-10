// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
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

            public Task UnlockAsync(int newVersion)
            {
                lock (lockObject)
                {
                    isLocked = false;

                    version = newVersion;
                }

                return TaskHelper.Done;
            }
        }

        public MigratorTests()
        {
            A.CallTo(() => path.GetNext(A<int>.Ignored))
                .ReturnsLazily((int v) =>
                {
                    var m = migrations.Where(x => x.From == v).ToList();

                    return m.Count == 0 ? (0, null) : (migrations.Max(x => x.To), migrations.Select(x => x.Migration));
                });

            A.CallTo(() => status.GetVersionAsync()).Returns(0);
            A.CallTo(() => status.TryLockAsync()).Returns(true);
        }

        [Fact]
        public async Task Should_migrate_step_by_step()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);
            var migrator_2_3 = BuildMigration(2, 3);

            var sut = new Migrator(status, path, log);

            await sut.MigrateAsync();

            A.CallTo(() => migrator_0_1.UpdateAsync()).MustHaveHappened();
            A.CallTo(() => migrator_1_2.UpdateAsync()).MustHaveHappened();
            A.CallTo(() => migrator_2_3.UpdateAsync()).MustHaveHappened();

            A.CallTo(() => status.UnlockAsync(3)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_unlock_when_failed()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);
            var migrator_2_3 = BuildMigration(2, 3);

            var sut = new Migrator(status, path, log);

            A.CallTo(() => migrator_1_2.UpdateAsync()).Throws(new ArgumentException());

            await Assert.ThrowsAsync<ArgumentException>(sut.MigrateAsync);

            A.CallTo(() => migrator_0_1.UpdateAsync()).MustHaveHappened();
            A.CallTo(() => migrator_1_2.UpdateAsync()).MustHaveHappened();
            A.CallTo(() => migrator_2_3.UpdateAsync()).MustNotHaveHappened();

            A.CallTo(() => status.UnlockAsync(0)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_prevent_multiple_updates()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);

            var sut = new Migrator(new InMemoryStatus(), path, log) { LockWaitMs = 2 };

            await Task.WhenAll(Enumerable.Repeat(0, 10).Select(x => Task.Run(sut.MigrateAsync)));

            A.CallTo(() => migrator_0_1.UpdateAsync()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => migrator_1_2.UpdateAsync()).MustHaveHappened(Repeated.Exactly.Once);
        }

        private IMigration BuildMigration(int fromVersion, int toVersion)
        {
            var migration = A.Fake<IMigration>();

            migrations.Add((fromVersion, toVersion, migration));

            return migration;
        }
    }
}
