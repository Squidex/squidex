﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using TestSuite.Model;
using TestSuite.Utils;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class BackupTests : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();
    private readonly string appNameRestore = $"{Guid.NewGuid()}-restore";
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
        var backupUrl = TestHelpers.GetAndPrintValue("config:backupUrl", _.Url);

        // STEP 1: Create app
        var app = await _.PostAppAsync(appName);


        // STEP 2: Prepare app.
        await PrepareAppAsync(app);


        // STEP 3: Create backup
        await app.Backups.PostBackupAsync();

        var backups = await app.Backups.WaitForBackupsAsync(x => x.Status is JobStatus.Completed or JobStatus.Failed, TimeSpan.FromMinutes(2));
        var backup = backups.FirstOrDefault(x => x.Status is JobStatus.Completed or JobStatus.Failed);

        Assert.Equal(JobStatus.Completed, backup?.Status);


        // STEP 4: Restore backup
        var uri = new Uri(new Uri(backupUrl), backup._links["download"].Href);

        var restoreRequest = new RestoreRequestDto
        {
            Url = uri,
            // Choose a new app name, because the old one is deleted.
            Name = appNameRestore
        };

        await _.Client.Backups.PostRestoreJobAsync(restoreRequest);


        // STEP 5: Wait for the backup.
        var restore = await app.Backups.WaitForRestoreAsync(x => x.Url == uri && x.Status is JobStatus.Completed or JobStatus.Failed, TimeSpan.FromMinutes(2));

        Assert.Equal(JobStatus.Completed, restore?.Status);
    }

    [Fact]
    public async Task Should_backup_and_restore_app_with_deleted_app()
    {
        // Load the backup from another URL, because the public URL is might not be accessible for the server.
        var backupUrl = TestHelpers.GetAndPrintValue("config:backupUrl", _.Url);

        // STEP 1: Create app
        var app = await _.PostAppAsync(appNameRestore);


        // STEP 2: Prepare app.
        await PrepareAppAsync(app);


        // STEP 3: Create backup
        await app.Backups.PostBackupAsync();

        var backups = await app.Backups.WaitForBackupsAsync(x => x.Status is JobStatus.Completed or JobStatus.Failed, TimeSpan.FromMinutes(2));
        var backup = backups.FirstOrDefault(x => x.Status is JobStatus.Completed or JobStatus.Failed);

        Assert.Equal(JobStatus.Completed, backup?.Status);


        // STEP 4: Delete app
        await app.Apps.DeleteAppAsync();


        // STEP 5: Restore backup
        var uri = new Uri(new Uri(backupUrl), backup._links["download"].Href);

        var restoreRequest = new RestoreRequestDto
        {
            Url = uri,
            // Restore the old app name, because it has been deleted anyway.
            Name = appName
        };

        await app.Backups.PostRestoreJobAsync(restoreRequest);


        // STEP 6: Wait for the backup.
        var restore = await app.Backups.WaitForRestoreAsync(x => x.Url == uri && x.Status is JobStatus.Completed or JobStatus.Failed, TimeSpan.FromMinutes(2));

        Assert.Equal(JobStatus.Completed, restore?.Status);
    }

    private async Task PrepareAppAsync(ISquidexClient app)
    {
        // Create a test schema.
        await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        var contents = _.Client.Contents<TestEntity, TestEntityData>(schemaName);

        await contents.CreateAsync(new TestEntityData
        {
            Number = 1
        });


        // Upload a test asset
        var fileInfo = new FileInfo("Assets/logo-squared.png");

        await using (var stream = fileInfo.OpenRead())
        {
            var upload = new FileParameter(stream, fileInfo.Name, "image/png");

            await app.Assets.PostAssetAsync(file: upload);
        }


        // Create a workflow
        var workflowRequest = new AddWorkflowDto
        {
            Name = appName
        };

        await app.Apps.PostWorkflowAsync( workflowRequest);


        // Create a language
        var languageRequest = new AddLanguageDto
        {
            Language = "de"
        };

        await app.Apps.PostLanguageAsync(languageRequest);
    }
}
