// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using SchemaField = Squidex.Domain.Apps.Entities.Schemas.Commands.UpsertSchemaField;
using SchemaFieldRules = Squidex.Domain.Apps.Core.Schemas.FieldRules;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands;

public interface IUpsertCommand
{
    bool IsPublished { get; set; }

    string Category { get; set; }

    SchemaField[]? Fields { get; set; }

    SchemaScripts? Scripts { get; set; }

    SchemaProperties Properties { get; set; }

    FieldNames? FieldsInReferences { get; set; }

    FieldNames? FieldsInLists { get; set; }

    FieldRuleCommand[]? FieldRules { get; set; }

    ReadonlyDictionary<string, string>? PreviewUrls { get; set; }

    Schema ToSchema(string name, SchemaType type)
    {
        var fields = new List<RootField>();

        if (Fields?.Length > 0)
        {
            var totalFields = 0;

            foreach (var eventField in Fields)
            {
                totalFields++;

                var partitioning = Partitioning.FromString(eventField.Partitioning);

                var field = eventField.Properties.CreateRootField(totalFields, eventField.Name, partitioning) with
                {
                    IsLocked = eventField.IsLocked,
                    IsHidden = eventField.IsHidden,
                    IsDisabled = eventField.IsDisabled
                };

                if (field is ArrayField arrayField && eventField.Nested?.Length > 0)
                {
                    var arrayFields = new List<NestedField>();

                    foreach (var nestedEventField in eventField.Nested)
                    {
                        totalFields++;

                        var nestedField = nestedEventField.Properties.CreateNestedField(totalFields, nestedEventField.Name) with
                        {
                            IsLocked = nestedEventField.IsLocked,
                            IsHidden = nestedEventField.IsHidden,
                            IsDisabled = nestedEventField.IsDisabled
                        };

                        arrayFields.Add(nestedField);
                    }

                    field = arrayField with { FieldCollection = FieldCollection<NestedField>.Create(arrayFields.ToArray()) };
                }

                fields.Add(field);
            }
        }

        var schema = new Schema
        {
            Name = name,
            Category = Category,
            FieldCollection = FieldCollection<RootField>.Create(fields.ToArray()),
            FieldRules = SchemaFieldRules.Create(FieldRules?.Select(x => x.ToFieldRule()).ToArray()),
            FieldsInLists = FieldsInLists ?? FieldNames.Empty,
            FieldsInReferences = FieldsInReferences ?? FieldNames.Empty,
            IsPublished = IsPublished,
            PreviewUrls = PreviewUrls ?? ReadonlyDictionary.Empty<string, string>(),
            Properties = Properties ?? new SchemaProperties(),
            Scripts = Scripts ?? new SchemaScripts(),
            Type = type,
        };

        return schema;
    }
}
