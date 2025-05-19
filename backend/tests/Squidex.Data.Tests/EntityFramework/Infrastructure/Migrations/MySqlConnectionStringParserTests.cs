// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Providers.MySql;

namespace Squidex.EntityFramework.Infrastructure.Migrations;

public class MySqlConnectionStringParserTests
{
    [Fact]
    public void Should_parse_host_name()
    {
        var sut = new MySqlConnectionStringParser();

        var result = sut.GetHostName("Server=localhost;Port=33060;Database=test;User=mysql;Password=mysql");

        Assert.Equal("localhost", result);
    }
}
