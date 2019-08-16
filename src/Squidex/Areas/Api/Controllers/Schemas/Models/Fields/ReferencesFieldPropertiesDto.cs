﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields
{
    public sealed class ReferencesFieldPropertiesDto : FieldPropertiesDto
    {
        /// <summary>
        /// The minimum allowed items for the field value.
        /// </summary>
        public int? MinItems { get; set; }

        /// <summary>
        /// The maximum allowed items for the field value.
        /// </summary>
        public int? MaxItems { get; set; }

        /// <summary>
        /// True, if duplicate values are allowed.
        /// </summary>
        public bool AllowDuplicates { get; set; }

        /// <summary>
        /// True to resolve references in the content list.
        /// </summary>
        public bool ResolveReference { get; set; }

        /// <summary>
        /// The editor that is used to manage this field.
        /// </summary>
        public ReferencesFieldEditor Editor { get; set; }

        /// <summary>
        /// The id of the referenced schema.
        /// </summary>
        public Guid SchemaId { get; set; }

        public override FieldProperties ToProperties()
        {
            return SimpleMapper.Map(this, new ReferencesFieldProperties());
        }
    }
}
