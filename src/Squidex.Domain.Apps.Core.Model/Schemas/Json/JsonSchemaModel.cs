// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class JsonSchemaModel
    {
        private static readonly RootField[] Empty = new RootField[0];

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public bool IsPublished { get; set; }

        [JsonProperty]
        public SchemaProperties Properties { get; set; }

        [JsonProperty]
        public List<JsonFieldModel> Fields { get; set; }

        public JsonSchemaModel()
        {
        }

        public JsonSchemaModel(Schema schema)
        {
            Name = schema.Name;

            Properties = schema.Properties;

            Fields =
                schema.Fields.Select(x =>
                    new JsonFieldModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Children = CreateChildren(x),
                        IsHidden = x.IsHidden,
                        IsLocked = x.IsLocked,
                        IsDisabled = x.IsDisabled,
                        Partitioning = x.Partitioning.Key,
                        Properties = x.RawProperties
                    }).ToList();

            IsPublished = schema.IsPublished;
        }

        private static List<JsonNestedFieldModel> CreateChildren(IField field)
        {
            if (field is ArrayField arrayField)
            {
                return arrayField.Fields.Select(x =>
                    new JsonNestedFieldModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsHidden = x.IsHidden,
                        IsDisabled = x.IsDisabled,
                        Properties = x.RawProperties
                    }).ToList();
            }

            return null;
        }

        public Schema ToSchema(FieldRegistry registry)
        {
            var fields = Empty;

            if (Fields != null)
            {
                fields = new RootField[Fields.Count];

                for (var i = 0; i < fields.Length; i++)
                {
                    var fieldModel = Fields[i];

                    var parititonKey = new Partitioning(fieldModel.Partitioning);

                    var field = registry.CreateRootField(fieldModel.Id, fieldModel.Name, parititonKey, fieldModel.Properties);

                    if (field is ArrayField arrayField && fieldModel.Children?.Count > 0)
                    {
                        foreach (var nestedFieldModel in fieldModel.Children)
                        {
                            var nestedField = registry.CreateNestedField(nestedFieldModel.Id, nestedFieldModel.Name, nestedFieldModel.Properties);

                            if (nestedFieldModel.IsHidden)
                            {
                                nestedField = nestedField.Hide();
                            }

                            if (nestedFieldModel.IsDisabled)
                            {
                                nestedField = nestedField.Disable();
                            }

                            arrayField = arrayField.AddField(nestedField);
                        }

                        field = arrayField;
                    }

                    if (fieldModel.IsDisabled)
                    {
                        field = field.Disable();
                    }

                    if (fieldModel.IsLocked)
                    {
                        field = field.Lock();
                    }

                    if (fieldModel.IsHidden)
                    {
                        field = field.Hide();
                    }

                    fields[i] = field;
                }
            }

            return new Schema(Name, fields, Properties, IsPublished);
        }
    }
}