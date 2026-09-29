// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class SchemaTextExtractionStrategy(IEnumerable<IFieldTextStrategy> fieldStrategies, IEnumerable<ITextNormalizer> normalizers) : ITextExtractionStrategy
{
    private readonly IFieldTextStrategy[] fieldStrategies = fieldStrategies.OrderBy(x => x.Order).ToArray();
    private readonly ITextNormalizer[] normalizers = normalizers.OrderBy(x => x.Order).ToArray();

    public int Order => 0;

    public Dictionary<string, string>? Extract(TextExtractionContext context, DomainId contentId, ContentData data)
    {
        using var collector = new TextCollector(fieldStrategies, normalizers, context);

        var schema = context.Schema;

        var titleFields = context.GetOrAdd(typeof(SchemaTextExtractionStrategy), () => GetTitleFields(schema));
        foreach (var (fieldName, fieldData) in data)
        {
            if (fieldData == null)
            {
                continue;
            }

            RootField? field = null;
            schema?.FieldsByName.TryGetValue(fieldName, out field);

            foreach (var (language, value) in fieldData)
            {
                collector.Language = language;
                collector.IsTitle = titleFields.Contains(fieldName);

                collector.AppendField(field, value);
            }
        }

        return collector.Build();
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
