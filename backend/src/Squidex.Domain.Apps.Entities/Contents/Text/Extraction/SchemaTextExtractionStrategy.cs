// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class SchemaTextExtractionStrategy(IEnumerable<IFieldTextStrategy> fieldStrategies, IEnumerable<ITextNormalizer> normalizers) : ITextExtractionStrategy
{
    private readonly IFieldTextStrategy[] fieldStrategies = fieldStrategies.OrderBy(x => x.Order).ToArray();
    private readonly ITextNormalizer[] normalizers = normalizers.OrderBy(x => x.Order).ToArray();

    public int Order => 0;

    public ValueTask<Dictionary<string, string>?> ExtractAsync(TextExtractionContext context,
        CancellationToken ct)
    {
        using var collector = new TextCollector();

        var schema = context.Schema;
        var titleFields = GetTitleFields(schema);

        foreach (var (fieldName, fieldData) in context.Data)
        {
            if (fieldData == null)
            {
                continue;
            }

            RootField? field = null;
            schema?.FieldsByName.TryGetValue(fieldName, out field);

            var isTitle = titleFields.Contains(fieldName);

            foreach (var (language, value) in fieldData)
            {
                var fieldContext = new FieldTextContext(fieldStrategies, normalizers, collector, context.Components, language, isTitle);

                fieldContext.AppendField(field, value);
            }
        }

        return new ValueTask<Dictionary<string, string>?>(collector.Build());
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
