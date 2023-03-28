// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb;

[Trait("Category", "Dependencies")]
public class ContentsQueryDedicatedIntegrationTests : ContentsQueryTestsBase, IClassFixture<ContentsQueryFixture_Dedicated>
{
    public ContentsQueryDedicatedIntegrationTests(ContentsQueryFixture_Dedicated fixture)
        : base(fixture)
    {
    }
}
