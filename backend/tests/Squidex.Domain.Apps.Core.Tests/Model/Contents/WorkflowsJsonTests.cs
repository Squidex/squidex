// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Model.Contents;

public class WorkflowsJsonTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var workflows = Workflows.Empty.Add(DomainId.NewGuid(), "my-workflow");

        var serialized = workflows.SerializeAndDeserialize();

        Assert.Equal(workflows, serialized);
    }
}
