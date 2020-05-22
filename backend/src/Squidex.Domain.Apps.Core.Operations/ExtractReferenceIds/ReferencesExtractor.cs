// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    internal sealed class ReferencesExtractor : IFieldVisitor<None>
    {
        private readonly HashSet<DomainId> result;
        private readonly int take;
        private IJsonValue? value;

        public HashSet<DomainId> Result
        {
            get { return result; }
        }

        public ReferencesExtractor(HashSet<DomainId> result, int take)
        {
            Guard.NotNull(result, nameof(result));

            this.result = result;

            this.take = take;
        }

        public void SetValue(IJsonValue? newValue)
        {
            value = newValue;
        }

        public None Visit(IArrayField field)
        {
            if (value is JsonArray array)
            {
                foreach (var item in array.OfType<JsonObject>())
                {
                    foreach (var nestedField in field.Fields)
                    {
                        if (item.TryGetValue(nestedField.Name, out var nestedValue))
                        {
                            SetValue(nestedValue);

                            nestedField.Accept(this);
                        }
                    }
                }
            }

            return None.Value;
        }

        public None Visit(IField<AssetsFieldProperties> field)
        {
            value.AddIds(result, take);

            return None.Value;
        }

        public None Visit(IField<ReferencesFieldProperties> field)
        {
            value.AddIds(result, take);

            return None.Value;
        }

        public None Visit(IField<BooleanFieldProperties> field)
        {
            return None.Value;
        }

        public None Visit(IField<DateTimeFieldProperties> field)
        {
            return None.Value;
        }

        public None Visit(IField<GeolocationFieldProperties> field)
        {
            return None.Value;
        }

        public None Visit(IField<JsonFieldProperties> field)
        {
            return None.Value;
        }

        public None Visit(IField<NumberFieldProperties> field)
        {
            return None.Value;
        }

        public None Visit(IField<StringFieldProperties> field)
        {
            return None.Value;
        }

        public None Visit(IField<TagsFieldProperties> field)
        {
            return None.Value;
        }

        public None Visit(IField<UIFieldProperties> field)
        {
            return None.Value;
        }
    }
}
