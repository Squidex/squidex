// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class FieldTextContext
{
    private readonly IReadOnlyList<IFieldTextStrategy> strategies;
    private readonly IReadOnlyList<ITextNormalizer> normalizers;
    private readonly TextCollector collector;

    public ResolvedComponents Components { get; }

    public string Language { get; }

    public bool IsTitle { get; }

    public FieldTextContext(IReadOnlyList<IFieldTextStrategy> strategies, IReadOnlyList<ITextNormalizer> normalizers, TextCollector collector, ResolvedComponents components, string language, bool isTitle)
    {
        this.strategies = strategies;
        this.normalizers = normalizers;
        this.collector = collector;

        Components = components;
        Language = language;
        IsTitle = isTitle;
    }

    public void AppendText(string text, IField? field = null)
    {
        foreach (var normalizer in normalizers)
        {
            var normalized = normalizer.Normalize(text, field);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return;
            }

            text = normalized;
        }

        collector.Append(text, Language, IsTitle);
    }

    public void AppendValue(JsonValue value)
    {
        AppendField(null, value);
    }

    public void AppendObject(JsonObject obj, Func<string, IField?> fields)
    {
        foreach (var (key, value) in obj)
        {
            if (key is Component.Discriminator or Component.Descriptor)
            {
                continue;
            }

            AppendField(fields(key), value);
        }
    }

    public void AppendField(IField? field, JsonValue value)
    {
        var searchMode = field?.RawProperties.SearchMode ?? FieldSearchMode.Default;
        if (searchMode == FieldSearchMode.Exclude)
        {
            return;
        }

        var context = this;

        if (searchMode == FieldSearchMode.Title && !IsTitle)
        {
            context = new FieldTextContext(strategies, normalizers, collector, Components, Language, true);
        }

        foreach (var strategy in strategies)
        {
            if (strategy.TryExtract(field, value, context))
            {
                return;
            }
        }
    }
}
