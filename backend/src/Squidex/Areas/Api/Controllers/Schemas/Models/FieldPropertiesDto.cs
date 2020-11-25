﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Validation;
using Squidex.Web.Json;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    [JsonConverter(typeof(TypedJsonInheritanceConverter<FieldPropertiesDto>), "fieldType")]
    [KnownType(nameof(Subtypes))]
    public abstract class FieldPropertiesDto
    {
        /// <summary>
        /// Optional label for the editor.
        /// </summary>
        [LocalizedStringLength(100)]
        public string? Label { get; set; }

        /// <summary>
        /// Localized label.
        /// </summary>
        public LocalizedValue<string>? DefaultValuesLabel { get; set; }

        /// <summary>
        /// Hints to describe the schema.
        /// </summary>
        [LocalizedStringLength(1000)]
        public string? Hints { get; set; }

        /// <summary>
        /// Localized Hints.
        /// </summary>
        public LocalizedValue<string>? DefaultValuesHints { get; set; }

        /// <summary>
        /// Placeholder to show when no value has been entered.
        /// </summary>
        [LocalizedStringLength(100)]
        public string? Placeholder { get; set; }

        /// <summary>
        /// Indicates if the field is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Indicates if the field is required when publishing.
        /// </summary>
        public bool IsRequiredOnPublish { get; set; }

        /// <summary>
        /// Indicates if the field should be rendered with half width only.
        /// </summary>
        public bool IsHalfWidth { get; set; }

        /// <summary>
        /// Optional url to the editor.
        /// </summary>
        public string? EditorUrl { get; set; }

        /// <summary>
        /// Tags for automation processes.
        /// </summary>
        public ReadOnlyCollection<string>? Tags { get; set; }

        public abstract FieldProperties ToProperties();

        public static Type[] Subtypes()
        {
            var type = typeof(FieldPropertiesDto);

            return type.Assembly.GetTypes().Where(type.IsAssignableFrom).ToArray();
        }
    }
}
