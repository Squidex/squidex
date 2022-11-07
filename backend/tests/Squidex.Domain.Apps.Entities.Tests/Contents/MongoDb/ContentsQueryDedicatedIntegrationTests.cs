// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb
{
    [Trait("Category", "Dependencies")]
    public class ContentsQueryDedicatedIntegrationTests : ContentsQueryTestsBase, IClassFixture<ContentsQueryDedicatedFixture>
    {
        public ContentsQueryDedicatedIntegrationTests(ContentsQueryDedicatedFixture fixture)
            : base(fixture)
        {
        }
    }
}
