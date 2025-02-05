// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Shared;

public abstract class ContentSnapshotStoreTests : SnapshotStoreTests<WriteContent>
{
    public GivenContext Context { get; } = new GivenContext();

    protected ContentSnapshotStoreTests()
    {
        Context.Schema = Context.Schema.AddReferences(0, "myReferences", Partitioning.Invariant);
    }

    protected override WriteContent CreateEntity(DomainId id, int version)
    {
        return Cleanup(Context.CreateWriteContent() with
        {
            Id = id,
            CurrentVersion = new ContentVersion(Status.Published,
                new ContentData()
                    .AddField("myString",
                        new ContentFieldData()
                            .AddInvariant("Hello Squidex"))
                    .AddField("myReferences",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Array(
                                    DomainId.NewGuid(),
                                    DomainId.NewGuid(),
                                    DomainId.NewGuid())))),
            Version = version,
        });
    }

    protected override WriteContent Cleanup(WriteContent expected)
    {
        return SimpleMapper.Map(expected, new WriteContent());
    }
}
