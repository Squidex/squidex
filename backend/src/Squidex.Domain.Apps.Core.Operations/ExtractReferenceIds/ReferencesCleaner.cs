﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public sealed class ReferencesCleaner : IFieldVisitor<IJsonValue>
    {
        private readonly HashSet<Guid> validIds;
        private IJsonValue value;

        public ReferencesCleaner(HashSet<Guid> validIds)
        {
            Guard.NotNull(validIds);

            this.validIds = validIds;
        }

        public void SetValue(IJsonValue newValue)
        {
            value = newValue;
        }

        public IJsonValue Visit(IField<AssetsFieldProperties> field)
        {
            return CleanIds();
        }

        public IJsonValue Visit(IField<ReferencesFieldProperties> field)
        {
            return CleanIds();
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

        public IJsonValue Visit(IField<UIFieldProperties> field)
        {
            return value;
        }

        public IJsonValue Visit(IArrayField field)
        {
            return value;
        }

        private IJsonValue CleanIds()
        {
            if (value is JsonArray array)
            {
                var result = new JsonArray(array);

                for (var i = 0; i < result.Count; i++)
                {
                    if (!IsValidReference(result[i]))
                    {
                        result.RemoveAt(i);
                        i--;
                    }
                }

                return result;
            }

            return value;
        }

        private bool IsValidReference(IJsonValue item)
        {
            return item is JsonString s && Guid.TryParse(s.Value, out var guid) && validIds.Contains(guid);
        }
    }
}
