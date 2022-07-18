// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using TestSuite.Model;
using TestSuite.Utils;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class BackupTests : IClassFixture<ClientFixture>
    {
        private readonly string appName = Guid.NewGuid().ToString();
        private readonly string schemaName = $"schema-{Guid.NewGuid()}";

        public ClientFixture _ { get; }

        public BackupTests(ClientFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_backup_and_restore_app()
        {
            // Load the backup from another URL, because the public URL is might not be accessible for the server.
            var backupUrl = TestHelpers.GetAndPrintValue("config:backupUrl", _.ServerUrl);

            var appNameRestore = $"{appName}-restore";

            // STEP 1: Create app
            var createRequest = new CreateAppDto { Name = appName };

            await _.Apps.PostAppAsync(createRequest);


            // STEP 2: Prepare app.
            await PrepareAppAsync(appName);


            // STEP 3: Create backup
            await _.Backups.PostBackupAsync(appName);

            var backup = await _.Backups.WaitForBackupAsync(appName, TimeSpan.FromMinutes(2));

            Assert.Equal(JobStatus.Completed, backup?.Status);


            // STEP 4: Restore backup
            var uri = new Uri(new Uri(backupUrl), backup._links["download"].Href);

            var restoreRequest = new RestoreRequestDto { Url = uri, Name = appNameRestore };

            await _.Backups.PostRestoreJobAsync(restoreRequest);


            // STEP 5: Wait for the backup.
            var restore = await _.Backups.WaitForRestoreAsync(uri, TimeSpan.FromMinutes(2));

            Assert.Equal(JobStatus.Completed, restore?.Status);
        }

        private async Task PrepareAppAsync(string appName)
        {
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
