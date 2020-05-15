// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    [Trait("Category", "NotAutomated")]
    public class BackupTests : IClassFixture<ClientFixture>
    {
        public ClientFixture _ { get; }

        public BackupTests(ClientFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_backup_and_restore_app()
        {
            var timeout = TimeSpan.FromMinutes(2);

            var appName = Guid.NewGuid().ToString();
            var appNameRestore = $"{appName}-restore";

            // STEP 1: Create app
            var createRequest = new CreateAppDto { Name = appName };

            await _.Apps.PostAppAsync(createRequest);


            // STEP 2: Create backup
            await _.Backups.PostBackupAsync(appName);

            BackupJobDto backup = null;

            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
                {
                    while (true)
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        await Task.Delay(1000);

                        var backups = await _.Backups.GetBackupsAsync(appName);

                        if (backups.Items.Count > 0)
                        {
                            backup = backups.Items.FirstOrDefault();

                            break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Assert.True(false, $"Could not retrieve backup within {timeout}.");
            }


            // STEP 3: Restore backup
            var uri = new Uri($"{_.ServerUrl}{backup._links["download"].Href}");

            var restoreRequest = new RestoreRequestDto { Url = uri, Name = appNameRestore };

            await _.Backups.PostRestoreJobAsync(restoreRequest);

            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
                {
                    while (true)
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        await Task.Delay(1000);

                        var job = await _.Backups.GetRestoreJobAsync();

                        if (job != null && job.Url == uri && job.Status == JobStatus.Completed)
                        {
                            break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Assert.True(false, $"Could not retrieve restored app within {timeout}.");
            }
        }
    }
}
