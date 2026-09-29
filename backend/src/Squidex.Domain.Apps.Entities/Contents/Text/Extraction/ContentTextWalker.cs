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

// Walks through a single content and passes the texts to the collector.
public sealed class ContentTextWalker(
    IReadOnlyList<ITextFieldStrategy> strategies,
    IReadOnlyList<ITextNormalizer> normalizers,
    TextExtractionContext context,
    TextCollector collector)
{
    private static readonly object TitleFieldsKey = new object();
    private string language = string.Empty;
    private bool isTitle;

    public TextExtractionContext Context => context;

    public void Walk(ContentData data)
    {
        var schema = context.Schema;

        var titleFields = context.GetOrAdd(TitleFieldsKey, schema, static x => GetTitleFields(x));
        foreach (var (fieldName, fieldData) in data)
        {
            if (fieldData == null)
            {
                continue;
            }

            RootField? field = null;
            schema?.FieldsByName.TryGetValue(fieldName, out field);

            isTitle = titleFields.Contains(fieldName);

            foreach (var (fieldLanguage, value) in fieldData)
            {
                // When the text is later appended we need the current language.
                language = fieldLanguage;

                AppendField(field, value);
            }
        }
    }

    public void AppendField(IField? field, JsonValue value)
    {
        var searchMode = field?.RawProperties.SearchMode ?? FieldSearchMode.Default;
        if (searchMode == FieldSearchMode.Exclude)
        {
            return;
        }

        var wasTitle = isTitle;
        try
        {
            isTitle |= searchMode == FieldSearchMode.Title;
            foreach (var strategy in strategies)
            {
                if (strategy.TryExtract(field, value, this))
                {
                    return;
                }
            }
        }
        finally
        {
            // The title mode only applies to the field and its nested values.
            isTitle = wasTitle;
        }
    }

    public void AppendValue(JsonValue value)
    {
        AppendField(null, value);
    }

    public void AppendObject<T>(JsonObject obj, IReadOnlyDictionary<string, T> fields) where T : IField
    {
        foreach (var (key, value) in obj)
        {
            if (key is Component.Discriminator or Component.Descriptor)
            {
                continue;
            }

            AppendField(fields.GetValueOrDefault(key), value);
        }
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

        collector.Append(text, language, isTitle);
    }

    private static HashSet<string> GetTitleFields(Schema? schema)
    {
        var result = new HashSet<string>();

        if (schema == null)
        {
            return result;
        }

        // The fields in the references are used to identify a content, so they are the best candidates for titles.
        var names = schema.FieldsInReferences.Count > 0 ? schema.FieldsInReferences : schema.FieldsInLists;

        foreach (var name in names)
        {
            if (FieldNames.IsDataField(name, out var dataField))
            {
                result.Add(dataField);
            }
            else if (!FieldNames.IsMetaField(name))
            {
                result.Add(name);
            }
        }

        // Use the same fallback as the UI, which shows the first field if nothing is configured.
        if (result.Count == 0 && schema.Fields.Count > 0)
        {
            result.Add(schema.Fields[0].Name);
        }

        return result;
    }
}
