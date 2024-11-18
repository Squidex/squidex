// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class HistoryTests(CreatedAppFixture fixture) : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_get_history()
    {
        var history = await _.Client.History.PollAsync(null, x => true);

        Assert.NotNull(history);
    }
}
