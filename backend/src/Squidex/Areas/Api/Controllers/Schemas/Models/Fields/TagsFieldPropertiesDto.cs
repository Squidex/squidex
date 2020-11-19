// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields
{
    public sealed class TagsFieldPropertiesDto : FieldPropertiesDto
    {
        /// <summary>
        /// The language specific default value for the field value.
        /// </summary>
        public LocalizedValue<string[]?> DefaultValues { get; set; }

        /// <summary>
        /// The default value.
        /// </summary>
        public string[]? DefaultValue { get; set; }

        /// <summary>
        /// The minimum allowed items for the field value.
        /// </summary>
        public int? MinItems { get; set; }

        /// <summary>
        /// The maximum allowed items for the field value.
        /// </summary>
        public int? MaxItems { get; set; }

        /// <summary>
        /// The allowed values for the field value.
        /// </summary>
        public ReadOnlyCollection<string>? AllowedValues { get; set; }

        /// <summary>
        /// The editor that is used to manage this field.
        /// </summary>
        public TagsFieldEditor Editor { get; set; }

        public override FieldProperties ToProperties()
        {
            var result = SimpleMapper.Map(this, new TagsFieldProperties());

            return result;
        }
    }
}
