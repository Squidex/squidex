// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using TestSuite.Model;
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

            var appNameSource = Guid.NewGuid().ToString();
            var appNameRestore = $"{appNameSource}-restore";

            // STEP 1: Create app
            var createRequest = new CreateAppDto { Name = appNameSource };

            await _.Apps.PostAppAsync(createRequest);


            // STEP 2: Prepare app.
            await PrepareAppAsync(appNameSource);


            // STEP 3: Create backup
            await _.Backups.PostBackupAsync(appNameSource);

            BackupJobDto backup = null;
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
                {
                    while (true)
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        await Task.Delay(1000);

                        var foundBackup = (await _.Backups.GetBackupsAsync(appNameSource)).Items.FirstOrDefault();

                        if (foundBackup?.Status == JobStatus.Completed)
                        {
                            backup = foundBackup;
                            break;
                        }
                        else if (foundBackup?.Status == JobStatus.Failed)
                        {
                            throw new InvalidOperationException("Backup operation failed.");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Assert.True(false, $"Could not retrieve backup within {timeout}.");
            }


            // STEP 3: Restore backup
            var uri = new Uri(new Uri(_.ServerUrl, UriKind.Absolute), backup._links["download"].Href);

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

                        var foundRestore = await _.Backups.GetRestoreJobAsync();

                        if (foundRestore?.Url == uri && foundRestore.Status == JobStatus.Completed)
                        {
                            break;
                        }
                        else if (foundRestore?.Url == uri && foundRestore.Status == JobStatus.Failed)
                        {
                            throw new InvalidOperationException("Restore operation failed.");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Assert.True(false, $"Could not retrieve restored app within {timeout}.");
            }
        }

        private async Task PrepareAppAsync(string appName)
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // Create a test schema.
            await TestEntity.CreateSchemaAsync(_.Schemas, appName, schemaName);

            var contents = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(appName, schemaName);

            await contents.CreateAsync(new TestEntityData { Number = 1 });


            // Upload a test asset
            var fileInfo = new FileInfo("Assets/logo-squared.png");

            await using (var stream = fileInfo.OpenRead())
            {
                var upload = new FileParameter(stream, fileInfo.Name, "image/png");

                await _.Assets.PostAssetAsync(appName, file: upload);
            }


            // Create a workflow
            var workflow = new AddWorkflowDto { Name = appName };

            await _.Apps.PostWorkflowAsync(appName, workflow);


            // Create a language
            var language = new AddLanguageDto { Language = "de" };

            await _.Apps.PostLanguageAsync(appName, language);
        }
    }
}
