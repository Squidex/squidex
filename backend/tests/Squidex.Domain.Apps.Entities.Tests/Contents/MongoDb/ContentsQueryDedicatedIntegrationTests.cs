// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb;

[Trait("Category", "Dependencies")]
public class ContentsQueryDedicatedIntegrationTests : ContentsQueryTestsBase, IClassFixture<ContentsQueryDedicatedFixture>
{
    public ContentsQueryDedicatedIntegrationTests(ContentsQueryDedicatedFixture fixture)
        : base(fixture)
    {
    }
}
