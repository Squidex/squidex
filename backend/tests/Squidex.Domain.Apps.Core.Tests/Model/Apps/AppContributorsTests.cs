// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Apps;

public class AppContributorsTests
{
    private readonly Contributors contributors_0 = Contributors.Empty;

    [Fact]
    public void Should_assign_new_contributor()
    {
        var contributors_1 = contributors_0.Assign("1", Role.Developer);
        var contributors_2 = contributors_1.Assign("2", Role.Editor);

        Assert.Equal(Role.Developer, contributors_2["1"]);
        Assert.Equal(Role.Editor, contributors_2["2"]);
    }

    [Fact]
    public void Should_replace_contributor_if_already_exists()
    {
        var contributors_1 = contributors_0.Assign("1", Role.Developer);
        var contributors_2 = contributors_1.Assign("1", Role.Owner);

        Assert.Equal(Role.Owner, contributors_2["1"]);
    }

    [Fact]
    public void Should_return_same_contributors_if_contributor_is_updated_with_same_role()
    {
        var contributors_1 = contributors_0.Assign("1", Role.Developer);
        var contributors_2 = contributors_1.Assign("1", Role.Developer);

        Assert.Same(contributors_1, contributors_2);
    }

    [Fact]
    public void Should_remove_contributor()
    {
        var contributors_1 = contributors_0.Assign("1", Role.Developer);
        var contributors_2 = contributors_1.Assign("2", Role.Developer);
        var contributors_3 = contributors_2.Assign("3", Role.Developer);
        var contributors_4 = contributors_3.Remove("2");

        Assert.Equal(new[] { "1", "3" }, contributors_4.Keys);
    }

    [Fact]
    public void Should_do_nothing_if_contributor_to_remove_not_found()
    {
        var contributors_1 = contributors_0.Remove("2");

        Assert.Same(contributors_0, contributors_1);
    }
}
