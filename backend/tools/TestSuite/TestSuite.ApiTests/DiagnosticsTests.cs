// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class DiagnosticsTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; }

    public DiagnosticsTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_gc_dump()
    {
        await _.Diagnostics.GetGCDumpAsync();
    }
}
