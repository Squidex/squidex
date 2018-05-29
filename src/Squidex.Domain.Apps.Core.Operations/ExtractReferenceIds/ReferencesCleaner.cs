// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public sealed class ReferencesCleaner : IFieldVisitor<JToken>
    {
        private readonly JToken value;
        private readonly ICollection<Guid> oldReferences;

        private ReferencesCleaner(JToken value, ICollection<Guid> oldReferences)
        {
            this.value = value;

            this.oldReferences = oldReferences;
        }

        public static JToken CleanReferences(IField field, JToken value, ICollection<Guid> oldReferences)
        {
            return field.Accept(new ReferencesCleaner(value, oldReferences));
        }

        public JToken Visit(IArrayField field)
        {
            return value;
        }

        public JToken Visit(IField<AssetsFieldProperties> field)
        {
            return CleanIds();
        }

        public JToken Visit(IField<ReferencesFieldProperties> field)
        {
            if (oldReferences.Contains(field.Properties.SchemaId))
            {
                return new JArray();
            }

            return CleanIds();
        }

        private JToken CleanIds()
        {
            var ids = value.ToGuidSet();

            var isRemoved = false;

            foreach (var oldReference in oldReferences)
            {
                isRemoved |= ids.Remove(oldReference);
            }

            return isRemoved ? ids.ToJToken() : value;
        }

        public JToken Visit(IField<BooleanFieldProperties> field)
        {
            return value;
        }

        public JToken Visit(IField<DateTimeFieldProperties> field)
        {
            return value;
        }

        public JToken Visit(IField<GeolocationFieldProperties> field)
        {
            return value;
        }

        public JToken Visit(IField<JsonFieldProperties> field)
        {
            return value;
        }

        public JToken Visit(IField<NumberFieldProperties> field)
        {
            return value;
        }

        public JToken Visit(IField<StringFieldProperties> field)
        {
            return value;
        }

        public JToken Visit(IField<TagsFieldProperties> field)
        {
            return value;
        }
    }
}
