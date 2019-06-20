// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Areas.Api.Controllers.Schemas.Models.Fields;
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
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
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
        [Required]
        public string Partitioning { get; set; }

        /// <summary>
        /// The field properties.
        /// </summary>
        [Required]
        public FieldPropertiesDto Properties { get; set; }

        /// <summary>
        /// The nested fields.
        /// </summary>
        public List<NestedFieldDto> Nested { get; set; }

        public void CreateLinks(ApiController controller, string app, string schema, bool allowUpdate)
        {
            allowUpdate = allowUpdate && !IsLocked;

            if (allowUpdate)
            {
                var values = new { app, name = schema, id = FieldId };

                AddPutLink("update", controller.Url<SchemaFieldsController>(x => nameof(x.PutField), values));

                if (IsHidden)
                {
                    AddPutLink("show", controller.Url<SchemaFieldsController>(x => nameof(x.ShowField), values));
                }
                else
                {
                    AddPutLink("hide", controller.Url<SchemaFieldsController>(x => nameof(x.HideField), values));
                }

                if (IsDisabled)
                {
                    AddPutLink("enable", controller.Url<SchemaFieldsController>(x => nameof(x.EnableField), values));
                }
                else
                {
                    AddPutLink("disable", controller.Url<SchemaFieldsController>(x => nameof(x.DisableField), values));
                }

                if (Properties is ArrayFieldPropertiesDto)
                {
                    var parentValues = new { app, name = schema, parentId = FieldId };

                    AddPostLink("fields/add", controller.Url<SchemaFieldsController>(x => nameof(x.PostNestedField), parentValues));

                    AddPutLink("fields/order", controller.Url<SchemaFieldsController>(x => nameof(x.PutNestedFieldOrdering), parentValues));
                }

                AddPutLink("lock", controller.Url<SchemaFieldsController>(x => nameof(x.LockField), values));

                AddDeleteLink("delete", controller.Url<SchemaFieldsController>(x => nameof(x.DeleteField), values));
            }

            if (Nested != null)
            {
                foreach (var nested in Nested)
                {
                    nested.CreateLinks(controller, app, schema, FieldId, allowUpdate);
                }
            }
        }
    }
}
