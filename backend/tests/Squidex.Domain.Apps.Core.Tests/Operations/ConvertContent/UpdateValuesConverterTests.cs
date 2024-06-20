// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent;

public class UpdateValuesConverterTests
{
    private readonly IScriptEngine scriptEngine;

    public UpdateValuesConverterTests()
    {
        scriptEngine = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            }));
    }

    [Fact]
    public void Should_update_value()
    {
        var field1 = Fields.Number(1, "number1", Partitioning.Invariant);

        var schema =
            new Schema { Name = "my-schema" }
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en",
                            JsonValue.Object()
                                .Add("$update", "$data.number1.en + 1")));

        var existing =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 42));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new UpdateValues(existing, scriptEngine, true))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 43));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_update_field()
    {
        var field1 = Fields.Number(1, "number1", Partitioning.Invariant);

        var schema =
            new Schema { Name = "my-schema" }
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("$update", "{ \"en\": $data.number1.en + 1 }"));

        var existing =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 42));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new UpdateValues(existing, scriptEngine, true))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 43));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_update_with_existing_value()
    {
        var field1 = Fields.Number(1, "number1", Partitioning.Invariant);

        var schema =
            new Schema { Name = "my-schema" }
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en",
                            JsonValue.Object()
                                .Add("$update", "$data.number1.en + $self.increment")
                                .Add("increment", 7)));

        var existing =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 42));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new UpdateValues(existing, scriptEngine, true))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 49));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_unset_value()
    {
        var field1 = Fields.Number(1, "number1", Partitioning.Invariant);

        var schema =
            new Schema { Name = "my-schema" }
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en",
                            JsonValue.Object()
                                .Add("$unset", true)));

        var existing =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 42));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new UpdateValues(existing, scriptEngine, true))
                .Convert(source);

        var expected =
            new ContentData()
                .AddField(field1.Name, []);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_unset_field()
    {
        var field1 = Fields.Number(1, "number1", Partitioning.Invariant);

        var schema =
            new Schema { Name = "my-schema" }
                .AddField(field1);

        var source =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("$unset", true));

        var existing =
            new ContentData()
                .AddField(field1.Name,
                    new ContentFieldData()
                        .AddLocalized("en", 42));

        var actual =
            new ContentConverter(ResolvedComponents.Empty, schema)
                .Add(new UpdateValues(existing, scriptEngine, true))
                .Convert(source);

        var expected = new ContentData();

        Assert.Equal(expected, actual);
    }
}
