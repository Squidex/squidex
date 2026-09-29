// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Contents.Text.Extraction;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class TextExtractorTests
{
    private readonly DomainId componentId = DomainId.NewGuid();
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
    private readonly TextExtractor sut;

    public TextExtractorTests()
    {
        sut = CreateExtractor(scriptEngine);
    }

    public static TextExtractor CreateExtractor(IScriptEngine scriptEngine)
    {
        ITextFieldStrategy[] fieldStrategies =
        [
            new ArrayFieldTextStrategy(),
            new ComponentTextStrategy(),
            new JsonTextStrategy(),
            new RichTextFieldTextStrategy(),
            new SearchPathsFieldTextStrategy(),
        ];

        ITextNormalizer[] normalizers =
        [
            new HtmlTextNormalizer(),
            new MarkdownTextNormalizer(),
            new NoiseTextNormalizer(),
        ];

        return new TextExtractor(
        [
            new SchemaTextExtractionStrategy(fieldStrategies, normalizers),
            new ScriptTextExtractionStrategy(scriptEngine),
        ]);
    }

    [Fact]
    public void Should_extract_texts_by_language_without_schema()
    {
        var data =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddLocalized("en", "Hello")
                        .AddLocalized("de", "Hallo"))
                .AddField("field2",
                    new ContentFieldData()
                        .AddLocalized("en", "World"));

        var actual = Extract(data);

        Assert.Equal("Hello World", actual!.Texts!["en"]);
        Assert.Equal("Hallo", actual!.Texts!["de"]);
        Assert.Null(actual!.Titles);
    }

    [Fact]
    public void Should_return_null_if_no_text_found()
    {
        var data =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(42));

        var actual = Extract(data);

        Assert.Null(actual);
    }

    [Fact]
    public void Should_extract_all_strings_from_json_values()
    {
        var data =
            new ContentData()
                .AddField("json",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Object()
                                .Add("label", "Hello")
                                .Add("nested", JsonValue.Array(JsonValue.Object().Add("text", "World")))));

        var actual = Extract(data);

        Assert.Equal("Hello World", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Theory]
    [InlineData("a3d0e3c8-4c7b-4b54-8c9e-3a7c0d1f2e5b")]
    [InlineData("42")]
    [InlineData("3.14")]
    [InlineData("#ff0000")]
    [InlineData("#fff")]
    [InlineData("2024-01-15")]
    [InlineData("2024-01-15T10:30:00Z")]
    [InlineData("https://squidex.io/path?query=1")]
    [InlineData("data:image/png;base64,iVBORw0KGgo")]
    [InlineData("dGhpc2lzYXZlcnlsb25nYmFzZTY0c3RyaW5ndGhhdHNob3VsZGJlZmlsdGVyZWRvdXRieXRoZWZpbHRlcg==")]
    [InlineData("   ")]
    public void Should_detect_noise(string text)
    {
        Assert.True(NoiseTextNormalizer.IsNoise(text));
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("my-great-article")]
    [InlineData("Visit https://squidex.io for more")]
    [InlineData("42 is the answer")]
    [InlineData("mail@squidex.io")]
    public void Should_not_detect_text_as_noise(string text)
    {
        Assert.False(NoiseTextNormalizer.IsNoise(text));
    }

    [Fact]
    public void Should_skip_reference_ids()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddReferences(2, "references", Partitioning.Invariant);

        var data =
            new ContentData()
                .AddField("title",
                    new ContentFieldData()
                        .AddInvariant("Hello"))
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(DomainId.NewGuid().ToString(), DomainId.NewGuid().ToString())));

        var actual = Extract(data, schema);

        Assert.Equal("Hello", actual!.Titles![InvariantPartitioning.Key]);
        Assert.Null(actual!.Texts);
    }

    [Fact]
    public void Should_use_first_field_as_title_if_nothing_configured()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddString(2, "description", Partitioning.Invariant);

        var data =
            new ContentData()
                .AddField("description",
                    new ContentFieldData()
                        .AddInvariant("World"))
                .AddField("title",
                    new ContentFieldData()
                        .AddInvariant("Hello"));

        var actual = Extract(data, schema);

        Assert.Equal("Hello", actual!.Titles![InvariantPartitioning.Key]);
        Assert.Equal("World", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_use_fields_in_references_as_title()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "description", Partitioning.Invariant)
                .AddString(2, "title", Partitioning.Invariant)
                .SetFieldsInReferences(FieldNames.Create("data.title"))
                .SetFieldsInLists(FieldNames.Create("data.description"));

        var data =
            new ContentData()
                .AddField("description",
                    new ContentFieldData()
                        .AddInvariant("World"))
                .AddField("title",
                    new ContentFieldData()
                        .AddInvariant("Hello"));

        var actual = Extract(data, schema);

        Assert.Equal("Hello", actual!.Titles![InvariantPartitioning.Key]);
        Assert.Equal("World", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_use_fields_with_title_mode_as_title()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "description", Partitioning.Invariant)
                .AddJson(2, "json", Partitioning.Invariant,
                    new JsonFieldProperties { SearchMode = FieldSearchMode.Title })
                .SetFieldsInReferences(FieldNames.Create("data.other"));

        var data =
            new ContentData()
                .AddField("description",
                    new ContentFieldData()
                        .AddInvariant("World"))
                .AddField("json",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Object().Add("name", "Hello")));

        var actual = Extract(data, schema);

        Assert.Equal("Hello", actual!.Titles![InvariantPartitioning.Key]);
        Assert.Equal("World", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_skip_excluded_fields()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddString(2, "internal", Partitioning.Invariant,
                    new StringFieldProperties { SearchMode = FieldSearchMode.Exclude });

        var data =
            new ContentData()
                .AddField("title",
                    new ContentFieldData()
                        .AddInvariant("Hello"))
                .AddField("internal",
                    new ContentFieldData()
                        .AddInvariant("Secret"));

        var actual = Extract(data, schema);

        Assert.Equal("Hello", actual!.Titles![InvariantPartitioning.Key]);
        Assert.Null(actual!.Texts);
    }

    [Fact]
    public void Should_only_index_search_paths()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddJson(2, "json", Partitioning.Invariant,
                    new JsonFieldProperties { SearchPaths = ReadonlyList.Create("$.items[*].label", "author.name") });

        var data =
            new ContentData()
                .AddField("json",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Object()
                                .Add("items",
                                    JsonValue.Array(
                                        JsonValue.Object().Add("label", "Label1").Add("config", "Hidden1"),
                                        JsonValue.Object().Add("label", "Label2").Add("config", "Hidden2")))
                                .Add("author", JsonValue.Object().Add("name", "Sebastian"))
                                .Add("other", "Hidden3")));

        var actual = Extract(data, schema);

        Assert.Equal("Label1 Label2 Sebastian", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_apply_settings_of_nested_fields()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddArray(2, "array", Partitioning.Invariant, a => a
                    .AddString(21, "nested1")
                    .AddString(22, "nested2",
                        new StringFieldProperties { SearchMode = FieldSearchMode.Exclude }));

        var data =
            new ContentData()
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object().Add("nested1", "Hello").Add("nested2", "Hidden"))));

        var actual = Extract(data, schema);

        Assert.Equal("Hello", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_apply_settings_of_component_fields()
    {
        var componentSchema =
            new Schema { Name = "my-component" }
                .AddString(1, "visible", Partitioning.Invariant)
                .AddString(2, "hidden", Partitioning.Invariant,
                    new StringFieldProperties { SearchMode = FieldSearchMode.Exclude });

        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddComponent(2, "component", Partitioning.Invariant);

        var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [componentId] = componentSchema,
        });

        var data =
            new ContentData()
                .AddField("component",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Object()
                                .Add(Component.Discriminator, componentId.ToString())
                                .Add("visible", "Hello")
                                .Add("hidden", "Hidden")));

        var actual = Extract(data, schema, components);

        Assert.Equal("Hello", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_convert_markdown_to_text()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddString(2, "markdown", Partitioning.Invariant,
                    new StringFieldProperties { Editor = StringFieldEditor.Markdown });

        var data =
            new ContentData()
                .AddField("markdown",
                    new ContentFieldData()
                        .AddInvariant("# Hello **World**"));

        var actual = Extract(data, schema);

        Assert.Equal("Hello World", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_use_custom_field_strategy_before_default_strategies()
    {
        var customStrategy = A.Fake<ITextFieldStrategy>();

        A.CallTo(() => customStrategy.Order)
            .Returns(-1);

        A.CallTo(() => customStrategy.TryExtract(A<IField?>.That.Matches(x => x != null && x.Name == "custom"), A<JsonValue>._, A<ContentTextWalker>._))
            .Invokes(x => x.GetArgument<ContentTextWalker>(2)!.AppendText("Custom"))
            .Returns(true);

        var customExtractor = new TextExtractor([new SchemaTextExtractionStrategy([customStrategy, new JsonTextStrategy()], [])]);

        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddJson(2, "custom", Partitioning.Invariant,
                    new JsonFieldProperties { EditorUrl = "https://editor.io" });

        var data =
            new ContentData()
                .AddField("custom",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Object().Add("raw", "Hidden")));

        var actual = Extract(customExtractor, data, schema);

        Assert.Equal("Custom", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_apply_normalizers_in_order_and_exclude_dropped_texts()
    {
        var normalizer1 = A.Fake<ITextNormalizer>();
        var normalizer2 = A.Fake<ITextNormalizer>();

        A.CallTo(() => normalizer1.Order)
            .Returns(2);

        A.CallTo(() => normalizer1.Normalize(A<string>._, A<IField?>._))
            .ReturnsLazily(x => x.GetArgument<string>(0) == "Drop!" ? null : x.GetArgument<string>(0));

        A.CallTo(() => normalizer2.Order)
            .Returns(1);

        A.CallTo(() => normalizer2.Normalize(A<string>._, A<IField?>._))
            .ReturnsLazily(x => $"{x.GetArgument<string>(0)}!");

        var customExtractor = new TextExtractor([new SchemaTextExtractionStrategy([new JsonTextStrategy()], [normalizer1, normalizer2])]);

        var data =
            new ContentData()
                .AddField("field",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array("Drop", "Keep")));

        var actual = Extract(customExtractor, data);

        Assert.Equal("Keep!", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_not_convert_markdown_if_field_is_not_markdown()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddString(2, "input", Partitioning.Invariant);

        var data =
            new ContentData()
                .AddField("input",
                    new ContentFieldData()
                        .AddInvariant("C# **is** great"));

        var actual = Extract(data, schema);

        Assert.Equal("C# **is** great", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_convert_html_to_text()
    {
        var data =
            new ContentData()
                .AddField("html",
                    new ContentFieldData()
                        .AddInvariant("<p>Hello <strong>World</strong> &amp; <script>alert(1)</script>more</p>"));

        var actual = Extract(data);

        Assert.Equal("Hello World & more", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_convert_rich_text_to_text()
    {
        var schema =
            new Schema { Name = "my-schema" }
                .AddString(1, "title", Partitioning.Invariant)
                .AddRichText(2, "richText", Partitioning.Invariant);

        var data =
            new ContentData()
                .AddField("richText",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Object()
                                .Add("type", "doc")
                                .Add("content",
                                    JsonValue.Array(
                                        JsonValue.Object()
                                            .Add("type", "paragraph")
                                            .Add("content",
                                                JsonValue.Array(
                                                    JsonValue.Object()
                                                        .Add("type", "text")
                                                        .Add("text", "Hello World")))))));

        var actual = Extract(data, schema);

        Assert.Equal("Hello World", actual!.Texts![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_return_null_for_undefined_script_result()
    {
        var actual = ScriptTextExtractionStrategy.ToTexts(JsonValue.Null);

        Assert.Null(actual);
    }

    [Fact]
    public void Should_convert_string_script_result()
    {
        var actual = ScriptTextExtractionStrategy.ToTexts(JsonValue.Create("Hello World"));

        Assert.Equal("Hello World", actual!.Texts![InvariantPartitioning.Key]);
        Assert.Null(actual!.Titles);
    }

    [Fact]
    public void Should_convert_object_script_result()
    {
        var result =
            JsonValue.Object()
                .Add("title", "Hello")
                .Add("body",
                    JsonValue.Object()
                        .Add("en", "World")
                        .Add("de", "Welt"));

        var actual = ScriptTextExtractionStrategy.ToTexts(result);

        Assert.Equal("Hello", actual!.Titles![InvariantPartitioning.Key]);
        Assert.Equal("World", actual!.Texts!["en"]);
        Assert.Equal("Welt", actual!.Texts!["de"]);
    }

    [Fact]
    public void Should_use_script_before_schema()
    {
        var script = A.Fake<IScript>();

        A.CallTo(() => scriptEngine.CreateScript("script", A<ScriptOptions>._))
            .Returns(script);

        A.CallTo(() => script.Execute(A<ScriptVars>._))
            .Returns(JsonValue.Create("Scripted"));

        var actual = Extract(CreateData("Hello"), CreateScriptSchema());

        Assert.Equal("Scripted", actual!.Texts![InvariantPartitioning.Key]);
        Assert.Null(actual!.Titles);
    }

    [Fact]
    public void Should_fallback_to_schema_if_script_returns_null()
    {
        var script = A.Fake<IScript>();

        A.CallTo(() => scriptEngine.CreateScript("script", A<ScriptOptions>._))
            .Returns(script);

        A.CallTo(() => script.Execute(A<ScriptVars>._))
            .Returns(JsonValue.Null);

        var actual = Extract(CreateData("Hello"), CreateScriptSchema());

        Assert.Equal("Hello", actual!.Titles![InvariantPartitioning.Key]);
    }

    [Fact]
    public void Should_not_fallback_to_schema_if_script_returns_empty_object()
    {
        var script = A.Fake<IScript>();

        A.CallTo(() => scriptEngine.CreateScript("script", A<ScriptOptions>._))
            .Returns(script);

        A.CallTo(() => script.Execute(A<ScriptVars>._))
            .Returns(JsonValue.Object());

        var actual = Extract(CreateData("Hello"), CreateScriptSchema());

        Assert.Null(actual!.Texts);
        Assert.Null(actual!.Titles);
    }

    [Fact]
    public void Should_compile_script_only_once_per_context()
    {
        var script = A.Fake<IScript>();

        A.CallTo(() => scriptEngine.CreateScript("script", A<ScriptOptions>._))
            .Returns(script);

        A.CallTo(() => script.Execute(A<ScriptVars>._))
            .Returns(JsonValue.Create("Scripted"));

        using (var context = CreateContext(CreateScriptSchema()))
        {
            sut.Extract(context, DomainId.NewGuid(), CreateData("Hello"));
            sut.Extract(context, DomainId.NewGuid(), CreateData("World"));
        }

        A.CallTo(() => scriptEngine.CreateScript("script", A<ScriptOptions>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => script.Execute(A<ScriptVars>._))
            .MustHaveHappenedTwiceExactly();
        A.CallTo(() => script.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Should_recompile_script_after_error()
    {
        var script = A.Fake<IScript>();

        A.CallTo(() => scriptEngine.CreateScript("script", A<ScriptOptions>._))
            .Returns(script);

        A.CallTo(() => script.Execute(A<ScriptVars>._))
            .Throws(new InvalidOperationException());

        using var context = CreateContext(CreateScriptSchema());

        var actual1 = sut.Extract(context, DomainId.NewGuid(), CreateData("Hello"));
        var actual2 = sut.Extract(context, DomainId.NewGuid(), CreateData("World"));

        Assert.Equal("Hello", actual1!.Titles![InvariantPartitioning.Key]);
        Assert.Equal("World", actual2!.Titles![InvariantPartitioning.Key]);

        A.CallTo(() => scriptEngine.CreateScript("script", A<ScriptOptions>._))
            .MustHaveHappenedTwiceExactly();
        A.CallTo(() => script.Dispose())
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public void Should_fallback_to_schema_if_script_cannot_be_compiled()
    {
        A.CallTo(() => scriptEngine.CreateScript("script", A<ScriptOptions>._))
            .Throws(new InvalidOperationException());

        using var context = CreateContext(CreateScriptSchema());

        var actual1 = sut.Extract(context, DomainId.NewGuid(), CreateData("Hello"));
        var actual2 = sut.Extract(context, DomainId.NewGuid(), CreateData("World"));

        Assert.Equal("Hello", actual1!.Titles![InvariantPartitioning.Key]);
        Assert.Equal("World", actual2!.Titles![InvariantPartitioning.Key]);

        A.CallTo(() => scriptEngine.CreateScript("script", A<ScriptOptions>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Should_return_text_if_html_cannot_be_parsed()
    {
        var actual = new HtmlTextNormalizer().Normalize("<!-]</p>", null);

        Assert.NotNull(actual);
    }

    private static Schema CreateScriptSchema()
    {
        return new Schema { Name = "my-schema" }
            .AddString(1, "title", Partitioning.Invariant)
            .SetScripts(new SchemaScripts { Index = "script" });
    }

    private static ContentData CreateData(string title)
    {
        return new ContentData()
            .AddField("title",
                new ContentFieldData()
                    .AddInvariant(title));
    }

    private ExtractedTexts? Extract(ContentData data, Schema? schema = null, ResolvedComponents? components = null)
    {
        return Extract(sut, data, schema, components);
    }

    private static ExtractedTexts? Extract(TextExtractor extractor, ContentData data, Schema? schema = null, ResolvedComponents? components = null)
    {
        using var context = CreateContext(schema, components);

        return extractor.Extract(context, DomainId.NewGuid(), data);
    }

    private static TextExtractionContext CreateContext(Schema? schema, ResolvedComponents? components = null)
    {
        return new TextExtractionContext
        {
            AppId = NamedId.Of(DomainId.NewGuid(), "my-app"),
            Components = components ?? ResolvedComponents.Empty,
            Schema = schema,
            SchemaId = NamedId.Of(DomainId.NewGuid(), "my-schema"),
        };
    }
}
