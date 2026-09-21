// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class ScriptLogsTests(CreatedAppFixture fixture) : IClassFixture<CreatedAppFixture>
{
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";

    public CreatedAppFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_store_console_output_of_script()
    {
        var scripts = new SchemaScriptsDto
        {
            Create = @"
                    console.log('Hello', 42);
                    console.warn('World');",
        };

        // STEP 1: Create a schema.
        await TestEntity.CreateSchemaAsync(_.Client.Schemas, schemaName, scripts);


        // STEP 2: Create content.
        var contents = _.Client.Contents<TestEntity, TestEntityData>(schemaName);

        await contents.CreateAsync(new TestEntityData { Number = 13 });


        // STEP 3: Get the logs.
        var log = await PollAsync($"contents/{schemaName}");

        Assert.NotNull(log);
        Assert.Equal($"contents/{schemaName}/create", log.Name);
        Assert.Equal(2, log.TotalEntries);
        Assert.Equal([("log", "Hello 42"), ("warn", "World")], log.Entries.Select(x => (x.Level, x.Message)));
    }

    [Fact]
    public async Task Should_not_return_console_output_with_script_error()
    {
        var scripts = new SchemaScriptsDto
        {
            Create = @"
                    console.log('Secret');
                    reject('Failed');",
        };

        // STEP 1: Create a schema.
        await TestEntity.CreateSchemaAsync(_.Client.Schemas, schemaName, scripts);


        // STEP 2: Create content.
        var contents = _.Client.Contents<TestEntity, TestEntityData>(schemaName);

        var ex = await Assert.ThrowsAsync<SquidexException<ErrorDto>>(() =>
        {
            return contents.CreateAsync(new TestEntityData { Number = 13 });
        });

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("Failed", ex.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Secret", ex.ToString(), StringComparison.Ordinal);


        // STEP 3: Get the logs.
        var log = await PollAsync($"contents/{schemaName}");

        Assert.NotNull(log);
        Assert.Equal([("log", "Secret")], log.Entries.Select(x => (x.Level, x.Message)));
    }

    private async Task<ScriptLogDto?> PollAsync(string name)
    {
        // The logs are written in the background.
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        while (!cts.IsCancellationRequested)
        {
            var logs = await _.Client.ScriptLogs.GetScriptLogsAsync(name, null, null, cts.Token);

            if (logs.Items.Count > 0)
            {
                return logs.Items[0];
            }

            await Task.Delay(200, cts.Token);
        }

        return null;
    }
}
