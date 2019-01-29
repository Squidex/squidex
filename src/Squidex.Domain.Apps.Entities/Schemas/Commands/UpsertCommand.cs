// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using SchemaFields = System.Collections.Generic.List<Squidex.Domain.Apps.Entities.Schemas.Commands.UpsertSchemaField>;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public abstract class UpsertCommand : SchemaCommand
    {
        public bool IsPublished { get; set; }

        public string Category { get; set; }

        public SchemaFields Fields { get; set; }

        public SchemaScripts Scripts { get; set; }

        public SchemaProperties Properties { get; set; }

        public Dictionary<string, string> PreviewUrls { get; set; }

        public Schema ToSchema(string name, bool isSingleton)
        {
            var schema = new Schema(name, Properties, isSingleton);

            if (IsPublished)
            {
                schema = schema.Publish();
            }

            if (Scripts != null)
            {
                schema = schema.ConfigureScripts(Scripts);
            }

            if (PreviewUrls != null)
            {
                schema = schema.ConfigurePreviewUrls(PreviewUrls);
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

                    if (field is ArrayField arrayField && eventField.Nested?.Count > 0)
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
