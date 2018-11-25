// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public sealed class ReferencesCleaner : IFieldVisitor<IJsonValue>
    {
        private readonly IJsonValue value;
        private readonly ICollection<Guid> oldReferences;

        private ReferencesCleaner(IJsonValue value, ICollection<Guid> oldReferences)
        {
            this.value = value;

            this.oldReferences = oldReferences;
        }

        public static IJsonValue CleanReferences(IField field, IJsonValue value, ICollection<Guid> oldReferences)
        {
            return field.Accept(new ReferencesCleaner(value, oldReferences));
        }

        public IJsonValue Visit(IField<AssetsFieldProperties> field)
        {
            return CleanIds();
        }

        public IJsonValue Visit(IField<ReferencesFieldProperties> field)
        {
            if (oldReferences.Contains(field.Properties.SchemaId))
            {
                return JsonValue.Array();
            }

            return CleanIds();
        }

        private IJsonValue CleanIds()
        {
            var ids = value.ToGuidSet();

            var isRemoved = false;

            foreach (var oldReference in oldReferences)
            {
                isRemoved |= ids.Remove(oldReference);
            }

            return isRemoved ? ids.ToJsonArray() : value;
        }

        public IJsonValue Visit(IField<BooleanFieldProperties> field)
        {
            return value;
        }

        public IJsonValue Visit(IField<DateTimeFieldProperties> field)
        {
            return value;
        }

        public IJsonValue Visit(IField<GeolocationFieldProperties> field)
        {
            return value;
        }

        public IJsonValue Visit(IField<JsonFieldProperties> field)
        {
            return value;
        }

        public IJsonValue Visit(IField<NumberFieldProperties> field)
        {
            return value;
        }

        public IJsonValue Visit(IField<StringFieldProperties> field)
        {
            return value;
        }

        public IJsonValue Visit(IField<TagsFieldProperties> field)
        {
            return value;
        }

        public IJsonValue Visit(IArrayField field)
        {
            return value;
        }
    }
}
