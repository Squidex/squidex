// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Areas.Api.Controllers.Schemas.Models.Converters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class FieldDto : Resource
    {
        /// <summary>
        /// The id of the field.
        /// </summary>
        public long FieldId { get; set; }

        /// <summary>
        /// The name of the field. Must be unique within the schema.
        /// </summary>
        [LocalizedRequired]
        [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// Defines if the field is hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Defines if the field is locked.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Defines if the field is disabled.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Defines the partitioning of the field.
        /// </summary>
        [LocalizedRequired]
        public string Partitioning { get; set; }

        /// <summary>
        /// The field properties.
        /// </summary>
        [LocalizedRequired]
        public FieldPropertiesDto Properties { get; set; }

        /// <summary>
        /// The nested fields.
        /// </summary>
        public List<NestedFieldDto>? Nested { get; set; }

        public static NestedFieldDto FromField(NestedField field)
        {
            var properties = FieldPropertiesDtoFactory.Create(field.RawProperties);

            var result =
                SimpleMapper.Map(field,
                    new NestedFieldDto
                    {
                        FieldId = field.Id,
                        Properties = properties
                    });

            return result;
        }

        public static FieldDto FromField(RootField field)
        {
            var properties = FieldPropertiesDtoFactory.Create(field.RawProperties);

            var result =
                SimpleMapper.Map(field,
                    new FieldDto
                    {
                        FieldId = field.Id,
                        Properties = properties,
                        Partitioning = field.Partitioning.Key
                    });

            if (field is IArrayField arrayField)
            {
                result.Nested = new List<NestedFieldDto>();

                foreach (var nestedField in arrayField.Fields)
                {
                    result.Nested.Add(FromField(nestedField));
                }
            }

            return result;
        }

        public void CreateLinks(Resources resources, string schema, bool allowUpdate)
        {
            allowUpdate = allowUpdate && !IsLocked;

            if (allowUpdate)
            {
                var values = new { app = resources.App, schema, id = FieldId };

                AddPutLink("update", resources.Url<SchemaFieldsController>(x => nameof(x.PutField), values));

                if (IsHidden)
                {
                    AddPutLink("show", resources.Url<SchemaFieldsController>(x => nameof(x.ShowField), values));
                }
                else
                {
                    AddPutLink("hide", resources.Url<SchemaFieldsController>(x => nameof(x.HideField), values));
                }

                if (IsDisabled)
                {
                    AddPutLink("enable", resources.Url<SchemaFieldsController>(x => nameof(x.EnableField), values));
                }
                else
                {
                    AddPutLink("disable", resources.Url<SchemaFieldsController>(x => nameof(x.DisableField), values));
                }

                if (Nested != null)
                {
                    var parentValues = new { values.app, values.schema, parentId = FieldId };

                    AddPostLink("fields/add", resources.Url<SchemaFieldsController>(x => nameof(x.PostNestedField), parentValues));

                    if (Nested.Count > 0)
                    {
                        AddPutLink("fields/order", resources.Url<SchemaFieldsController>(x => nameof(x.PutNestedFieldOrdering), parentValues));
                    }
                }

                if (!IsLocked)
                {
                    AddPutLink("lock", resources.Url<SchemaFieldsController>(x => nameof(x.LockField), values));
                }

                AddDeleteLink("delete", resources.Url<SchemaFieldsController>(x => nameof(x.DeleteField), values));
            }

            if (Nested != null)
            {
                foreach (var nested in Nested)
                {
                    nested.CreateLinks(resources, schema, FieldId, allowUpdate);
                }
            }
        }
    }
}
