﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public sealed class ReferencesExtractor : IFieldVisitor<IEnumerable<Guid>>
    {
        private readonly IJsonValue value;

        private ReferencesExtractor(IJsonValue value)
        {
            this.value = value;
        }

        public static IEnumerable<Guid> ExtractReferences(IField field, IJsonValue value)
        {
            return field.Accept(new ReferencesExtractor(value));
        }

        public IEnumerable<Guid> Visit(IArrayField field)
        {
            var result = new List<Guid>();

            if (value is JsonArray array)
            {
                foreach (var item in array.OfType<JsonObject>())
                {
                    foreach (var nestedField in field.Fields)
                    {
                        if (item.TryGetValue(nestedField.Name, out var nestedValue))
                        {
                            result.AddRange(nestedField.Accept(new ReferencesExtractor(nestedValue)));
                        }
                    }
                }
            }

            return result;
        }

        public IEnumerable<Guid> Visit(IField<AssetsFieldProperties> field)
        {
            var ids = value.ToGuidSet();

            return ids;
        }

        public IEnumerable<Guid> Visit(IField<ReferencesFieldProperties> field)
        {
            var ids = value.ToGuidSet();

            if (field.Properties.SchemaId != Guid.Empty)
            {
                ids.Add(field.Properties.SchemaId);
            }

            return ids;
        }

        public IEnumerable<Guid> Visit(IField<BooleanFieldProperties> field)
        {
            return Enumerable.Empty<Guid>();
        }

        public IEnumerable<Guid> Visit(IField<DateTimeFieldProperties> field)
        {
            return Enumerable.Empty<Guid>();
        }

        public IEnumerable<Guid> Visit(IField<GeolocationFieldProperties> field)
        {
            return Enumerable.Empty<Guid>();
        }

        public IEnumerable<Guid> Visit(IField<JsonFieldProperties> field)
        {
            return Enumerable.Empty<Guid>();
        }

        public IEnumerable<Guid> Visit(IField<NumberFieldProperties> field)
        {
            return Enumerable.Empty<Guid>();
        }

        public IEnumerable<Guid> Visit(IField<StringFieldProperties> field)
        {
            return Enumerable.Empty<Guid>();
        }

        public IEnumerable<Guid> Visit(IField<TagsFieldProperties> field)
        {
            return Enumerable.Empty<Guid>();
        }

        public IEnumerable<Guid> Visit(IField<UIFieldProperties> field)
        {
            return Enumerable.Empty<Guid>();
        }
    }
}
