// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using SchemaField = Squidex.Domain.Apps.Entities.Schemas.Commands.UpsertSchemaField;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
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

        Dictionary<string, string>? PreviewUrls { get; set; }

        Schema ToSchema(string name, bool isSingleton)
        {
            var schema = new Schema(name, Properties, isSingleton);

            if (IsPublished)
            {
                schema = schema.Publish();
            }

            if (Scripts != null)
            {
                schema = schema.SetScripts(Scripts);
            }

            if (PreviewUrls != null)
            {
                schema = schema.SetPreviewUrls(PreviewUrls);
            }

            if (FieldsInLists != null)
            {
                schema = schema.SetFieldsInLists(FieldsInLists);
            }

            if (FieldsInReferences != null)
            {
                schema = schema.SetFieldsInReferences(FieldsInReferences);
            }

            if (FieldRules != null)
            {
                schema = schema.SetFieldRules(FieldRules.Select(x => x.ToFieldRule()).ToArray());
            }

            if (!string.IsNullOrWhiteSpace(Category))
            {
                schema = schema.ChangeCategory(Category);
            }

            var totalFields = 0;

            if (Fields != null)
            {
                foreach (var eventField in Fields)
                {
                    totalFields++;

                    var partitioning = Partitioning.FromString(eventField.Partitioning);

                    var field = eventField.Properties.CreateRootField(totalFields, eventField.Name, partitioning);

                    if (field is ArrayField arrayField && eventField.Nested?.Length > 0)
                    {
                        foreach (var nestedEventField in eventField.Nested)
                        {
                            totalFields++;

                            var nestedField = nestedEventField.Properties.CreateNestedField(totalFields, nestedEventField.Name);

                            if (nestedEventField.IsHidden)
                            {
                                nestedField = nestedField.Hide();
                            }

                            if (nestedEventField.IsDisabled)
                            {
                                nestedField = nestedField.Disable();
                            }

                            if (nestedEventField.IsLocked)
                            {
                                nestedField = nestedField.Lock();
                            }

                            arrayField = arrayField.AddField(nestedField);
                        }

                        field = arrayField;
                    }

                    if (eventField.IsHidden)
                    {
                        field = field.Hide();
                    }

                    if (eventField.IsDisabled)
                    {
                        field = field.Disable();
                    }

                    if (eventField.IsLocked)
                    {
                        field = field.Lock();
                    }

                    schema = schema.AddField(field);
                }
            }

            return schema;
        }
    }
}
