// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class HistoryTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; }

    public HistoryTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_get_history()
    {
        var history = await _.History.WaitForHistoryAsync(_.AppName, null, x => true, TimeSpan.FromSeconds(30));

        Assert.NotEmpty(history);
    }
}
