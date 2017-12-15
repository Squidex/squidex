// ==========================================================================
//  MigratorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Infrastructure.Migrations
{
    public sealed class MigratorTests
    {
        private readonly IMigrationStatus status = A.Fake<IMigrationStatus>();

        public MigratorTests()
        {
            A.CallTo(() => status.GetVersionAsync()).Returns(0);
            A.CallTo(() => status.TryLockAsync()).Returns(true);
        }

        [Fact]
        public async Task Should_migrate_step_by_step()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_1_2 = BuildMigration(1, 2);
            var migrator_2_3 = BuildMigration(2, 3);

            var migrator = new Migrator(status, new[] { migrator_0_1, migrator_1_2, migrator_2_3 }, A.Fake<ISemanticLog>());

            await migrator.MigrateAsync();

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

            var migrator = new Migrator(status, new[] { migrator_0_1, migrator_1_2, migrator_2_3 }, A.Fake<ISemanticLog>());

            A.CallTo(() => migrator_1_2.UpdateAsync()).Throws(new ArgumentException());

            await Assert.ThrowsAsync<ArgumentException>(migrator.MigrateAsync);

            A.CallTo(() => migrator_0_1.UpdateAsync()).MustHaveHappened();
            A.CallTo(() => migrator_1_2.UpdateAsync()).MustHaveHappened();
            A.CallTo(() => migrator_2_3.UpdateAsync()).MustNotHaveHappened();

            A.CallTo(() => status.UnlockAsync(1)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_migrate_with_fastest_path()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_0_2 = BuildMigration(0, 2);
            var migrator_1_2 = BuildMigration(1, 2);
            var migrator_2_3 = BuildMigration(2, 3);

            var migrator = new Migrator(status, new[] { migrator_0_1, migrator_0_2, migrator_1_2, migrator_2_3 }, A.Fake<ISemanticLog>());

            await migrator.MigrateAsync();

            A.CallTo(() => migrator_0_2.UpdateAsync()).MustHaveHappened();
            A.CallTo(() => migrator_0_1.UpdateAsync()).MustNotHaveHappened();
            A.CallTo(() => migrator_1_2.UpdateAsync()).MustNotHaveHappened();
            A.CallTo(() => migrator_2_3.UpdateAsync()).MustHaveHappened();

            A.CallTo(() => status.UnlockAsync(3)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_if_no_path_found()
        {
            var migrator_0_1 = BuildMigration(0, 1);
            var migrator_2_3 = BuildMigration(2, 3);

            var migrator = new Migrator(status, new[] { migrator_0_1, migrator_2_3 }, A.Fake<ISemanticLog>());

            await Assert.ThrowsAsync<InvalidOperationException>(migrator.MigrateAsync);

            A.CallTo(() => migrator_0_1.UpdateAsync()).MustNotHaveHappened();
            A.CallTo(() => migrator_2_3.UpdateAsync()).MustNotHaveHappened();

            A.CallTo(() => status.UnlockAsync(0)).MustHaveHappened();
        }

        private IMigration BuildMigration(int fromVersion, int toVersion)
        {
            var migration = A.Fake<IMigration>();

            A.CallTo(() => migration.FromVersion).Returns(fromVersion);
            A.CallTo(() => migration.ToVersion).Returns(toVersion);

            return migration;
        }
    }
}
