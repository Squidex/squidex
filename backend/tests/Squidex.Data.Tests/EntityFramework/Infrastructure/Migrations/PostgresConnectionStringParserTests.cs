// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Providers.Postgres;

namespace Squidex.EntityFramework.Infrastructure.Migrations;

public class PostgresConnectionStringParserTests
{
    [Fact]
    public void Should_parse_host_name()
    {
        var sut = new PostgresConnectionStringParser();

        var result = sut.GetHostName("Server=localhost;Port=54320;Database=test;User=postgres;Password=postgres");

        Assert.Equal("localhost", result);
    }
}
