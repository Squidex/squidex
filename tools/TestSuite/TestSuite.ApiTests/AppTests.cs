// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public sealed class AppTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; }

    public AppTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_get_app()
    {
        // STEP 1: Get app.
        var app = await _.Client.Apps.GetAppAsync();

        Assert.Equal(_.AppName, app.Name);
    }

    [Fact]
    public async Task Should_set_label()
    {
        // STEP 1: Update app.
        var updateRequest = new UpdateAppDto
        {
            Label = Guid.NewGuid().ToString()
        };

        var app_1 = await _.Client.Apps.PutAppAsync(updateRequest);

        Assert.Equal(updateRequest.Label, app_1.Label);
    }

    [Fact]
    public async Task Should_set_description()
    {
        // STEP 1: Update app.
        var updateRequest = new UpdateAppDto
        {
            Description = Guid.NewGuid().ToString()
        };

        var app_1 = await _.Client.Apps.PutAppAsync(updateRequest);

        Assert.Equal(updateRequest.Description, app_1.Description);
    }

    [Fact]
    public async Task Should_upload_image()
    {
        // STEP 1: Upload image.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var file = new FileParameter(stream, "logo-squared.png", "image/png");

            var app_1 = await _.Client.Apps.UploadImageAsync(file);

            // Should contain image link.
            Assert.True(app_1.Links.ContainsKey("image"));
        }


        // STEP 2: Download image.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var temp = new MemoryStream();

            var downloaded = new MemoryStream();

            using (var imageStream = await _.Client.Apps.GetImageAsync())
            {
                await imageStream.Stream.CopyToAsync(downloaded);
            }

            // Should dowload with correct size.
            Assert.True(downloaded.Length < stream.Length);
        }
    }

    [Fact]
    public async Task Should_delete_image()
    {
        // STEP 1: Upload image.
        await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
        {
            var file = new FileParameter(stream, "logo-squared.png", "image/png");

            var app_1 = await _.Client.Apps.UploadImageAsync(file);

            // Should contain image link.
            Assert.True(app_1.Links.ContainsKey("image"));
        }


        // STEP 2: Delete Image.
        var app_2 = await _.Client.Apps.DeleteImageAsync();

        // Should contain image link.
        Assert.False(app_2.Links.ContainsKey("image"));
    }

    [Fact]
    public async Task Should_get_settings()
    {
        // STEP 1: Get initial settings.
        var settings_0 = await _.Client.Apps.GetSettingsAsync();

        Assert.NotEmpty(settings_0.Patterns);
    }

    [Fact]
    public async Task Should_update_settings()
    {
        // STEP 1: Update settings with new state.
        var updateRequest = new UpdateAppSettingsDto
        {
            Patterns =
            [
                new PatternDto { Name = "pattern", Regex = ".*" }
            ],
            Editors =
            [
                new EditorDto { Name = "editor", Url = "http://squidex.io/path/to/editor" }
            ]
        };

        var settings_1 = await _.Client.Apps.PutSettingsAsync(updateRequest);

        Assert.NotEmpty(settings_1.Patterns);
        Assert.NotEmpty(settings_1.Editors);

        await Verify(settings_1)
            .IgnoreMember<AppSettingsDto>(x => x.Version);
    }
}
