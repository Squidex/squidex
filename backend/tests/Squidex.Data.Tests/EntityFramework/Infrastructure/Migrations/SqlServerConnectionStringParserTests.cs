// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Providers.SqlServer;

namespace Squidex.EntityFramework.Infrastructure.Migrations;

public class SqlServerConnectionStringParserTests
{
    [Fact]
    public void Should_parse_host_name()
    {
        var sut = new SqlServerConnectionStringParser();

        var result = sut.GetHostName("Server=localhost;Port=14330;Database=test;User=sa;Password=sqlserver");

        Assert.Equal("localhost", result);
    }
}
