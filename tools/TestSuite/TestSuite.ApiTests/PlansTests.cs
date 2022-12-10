// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class PlansTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; }

    public PlansTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_get_plans()
    {
        var plans = await _.Plans.GetPlansAsync(_.AppName);

        Assert.NotNull(plans);
    }
}
