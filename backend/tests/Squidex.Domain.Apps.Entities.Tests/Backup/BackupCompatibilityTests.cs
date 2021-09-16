// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class BackupCompatibilityTests
    {
        [Fact]
        public async Task Should_writer_version()
        {
            var writer = A.Fake<IBackupWriter>();

            await writer.WriteVersionAsync();

            A.CallTo(() => writer.WriteJsonAsync(A<string>._, A<CompatibilityExtensions.FileVersion>.That.Matches(x => x.Major == 5), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_throw_exception_if_backup_has_correct_version()
        {
            var reader = A.Fake<IBackupReader>();

            A.CallTo(() => reader.ReadJsonAsync<CompatibilityExtensions.FileVersion>(A<string>._, default))
                .Returns(new CompatibilityExtensions.FileVersion { Major = 5 });

            await reader.CheckCompatibilityAsync();
        }

        [Fact]
        public async Task Should_throw_exception_if_backup_has_wrong_version()
        {
            var reader = A.Fake<IBackupReader>();

            A.CallTo(() => reader.ReadJsonAsync<CompatibilityExtensions.FileVersion>(A<string>._, default))
                .Returns(new CompatibilityExtensions.FileVersion { Major = 3 });

            await Assert.ThrowsAsync<BackupRestoreException>(() => reader.CheckCompatibilityAsync());
        }

        [Fact]
        public async Task Should_not_throw_exception_if_backup_has_no_version()
        {
            var reader = A.Fake<IBackupReader>();

            A.CallTo(() => reader.ReadJsonAsync<CompatibilityExtensions.FileVersion>(A<string>._, default))
                .Throws(new FileNotFoundException());

            await reader.CheckCompatibilityAsync();
        }
    }
}
