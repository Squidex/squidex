// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.MongoDb.Domain.Contents;

[Trait("Category", "Dependencies")]
public class ContentsQueryDedicatedIntegrationTests(ContentsQueryFixture_Dedicated fixture)
    : ContentsQueryTestsBase(fixture), IClassFixture<ContentsQueryFixture_Dedicated>
{
}
