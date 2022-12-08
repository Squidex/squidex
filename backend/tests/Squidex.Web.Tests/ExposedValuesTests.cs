// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace Squidex.Web;

public class ExposedValuesTests
{
    [Fact]
    public void Should_create_from_configuration()
    {
        var source = new ExposedConfiguration
        {
            ["name1"] = "config1",
            ["name2"] = "config2",
            ["name3"] = "config3"
        };

        var configuration = A.Fake<IConfiguration>();

        SetupConfiguration(configuration, "config1", "value1");
        SetupConfiguration(configuration, "config2", "value2");

        var values = new ExposedValues(source, configuration);

        Assert.Equal(2, values.Count);
        Assert.Equal("value1", values["name1"]);
        Assert.Equal("value2", values["name2"]);
    }

    [Fact]
    public void Should_use_version_from_assembly()
    {
        var source = new ExposedConfiguration();

        var values = new ExposedValues(source, A.Fake<IConfiguration>(), typeof(ExposedValuesTests).Assembly);

        Assert.Equal("1.0.0.0", values["version"]);
    }

    [Fact]
    public void Should_format_empty_values()
    {
        var values = new ExposedValues();

        Assert.Empty(values.ToString());
    }

    [Fact]
    public void Should_format_from_single_value()
    {
        var values = new ExposedValues
        {
            ["name1"] = "value1"
        };

        Assert.Equal("name1: value1", values.ToString());
    }

    [Fact]
    public void Should_format_from_multiple_values()
    {
        var values = new ExposedValues
        {
            ["name1"] = "value1",
            ["name2"] = "value2"
        };

        Assert.Equal("name1: value1, name2: value2", values.ToString());
    }

    private static void SetupConfiguration(IConfiguration configuration, string key, string value)
    {
        var configSection = A.Fake<IConfigurationSection>();

        A.CallTo(() => configSection.Value).Returns(value);
        A.CallTo(() => configuration.GetSection(key)).Returns(configSection);
    }
}
