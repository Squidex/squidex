// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class NestedFieldDto : Resource
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
        /// The field properties.
        /// </summary>
        [LocalizedRequired]
        public FieldPropertiesDto Properties { get; set; }

        public void CreateLinks(Resources resources, string schema, long parentId, bool allowUpdate)
        {
            allowUpdate = allowUpdate && !IsLocked;

            if (allowUpdate)
            {
                var values = new { app = resources.App, schema, parentId, id = FieldId };

                AddPutLink("update", resources.Url<SchemaFieldsController>(x => nameof(x.PutNestedField), values));

                if (IsHidden)
                {
                    AddPutLink("show", resources.Url<SchemaFieldsController>(x => nameof(x.ShowNestedField), values));
                }
                else
                {
                    AddPutLink("hide", resources.Url<SchemaFieldsController>(x => nameof(x.HideNestedField), values));
                }

                if (IsDisabled)
                {
                    AddPutLink("enable", resources.Url<SchemaFieldsController>(x => nameof(x.EnableNestedField), values));
                }
                else
                {
                    AddPutLink("disable", resources.Url<SchemaFieldsController>(x => nameof(x.DisableNestedField), values));
                }

                if (!IsLocked)
                {
                    AddPutLink("lock", resources.Url<SchemaFieldsController>(x => nameof(x.LockNestedField), values));
                }

                AddDeleteLink("delete", resources.Url<SchemaFieldsController>(x => nameof(x.DeleteNestedField), values));
            }
        }
    }
}
