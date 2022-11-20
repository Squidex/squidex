// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;

namespace Squidex.Domain.Apps.Core.Model.Apps;

public class AppContributorsJsonTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var contributors = Contributors.Empty;

        contributors = contributors.Assign("1", Role.Developer);
        contributors = contributors.Assign("2", Role.Editor);
        contributors = contributors.Assign("3", Role.Owner);

        var serialized = contributors.SerializeAndDeserialize();

        Assert.Equal(contributors, serialized);
    }
}
