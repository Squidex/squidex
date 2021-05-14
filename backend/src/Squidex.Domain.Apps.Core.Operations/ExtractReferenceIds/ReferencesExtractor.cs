// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    internal sealed class ReferencesExtractor : IFieldVisitor<None, ReferencesExtractor.Args>
    {
        private static readonly ReferencesExtractor Instance = new ReferencesExtractor();

        public readonly struct Args
        {
            public readonly IJsonValue Value;

            public readonly HashSet<DomainId> Result;

            public readonly int ResultLimit;

            public Args(IJsonValue value, HashSet<DomainId> result, int take)
            {
                Value = value;
                Result = result;
                ResultLimit = take;
            }
        }

        private ReferencesExtractor()
        {
        }

        public static None Extract(IField field, IJsonValue? value, HashSet<DomainId> result, int take)
        {
            var args = new Args(value ?? JsonValue.Null, result, take);

            return field.Accept(Instance, args);
        }

        public None Visit(IArrayField field, Args args)
        {
            if (args.Value is JsonArray array)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    ExtractFromArrayItem(field, array[i], args);
                }
            }

            return None.Value;
        }

        public None Visit(IField<AssetsFieldProperties> field, Args args)
        {
            AddIds(ref args);

            return None.Value;
        }

        public None Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            AddIds(ref args);

            return None.Value;
        }

        public None Visit(IField<BooleanFieldProperties> field, Args args)
        {
            return None.Value;
        }

        public None Visit(IField<ComponentFieldProperties> field, Args args)
        {
            ExtractFromComponent(field, args.Value, args);

            return None.Value;
        }

        public None Visit(IField<ComponentsFieldProperties> field, Args args)
        {
            if (args.Value is JsonArray array)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    ExtractFromComponent(field, array[i], args);
                }
            }

            return None.Value;
        }

        public None Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            return None.Value;
        }

        public None Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            return None.Value;
        }

        public None Visit(IField<JsonFieldProperties> field, Args args)
        {
            return None.Value;
        }

        public None Visit(IField<NumberFieldProperties> field, Args args)
        {
            return None.Value;
        }

        public None Visit(IField<StringFieldProperties> field, Args args)
        {
            return None.Value;
        }

        public None Visit(IField<TagsFieldProperties> field, Args args)
        {
            return None.Value;
        }

        public None Visit(IField<UIFieldProperties> field, Args args)
        {
            return None.Value;
        }

        private void ExtractFromArrayItem(IArrayField field, IJsonValue value, Args args)
        {
            if (value is JsonObject obj)
            {
                foreach (var nestedField in field.Fields)
                {
                    if (obj.TryGetValue(nestedField.Name, out var nestedValue))
                    {
                        nestedField.Accept(this, new Args(nestedValue, args.Result, args.ResultLimit));
                    }
                }
            }
        }

        private void ExtractFromComponent(IField field, IJsonValue value, Args args)
        {
            if (value is JsonObject obj && obj.TryGetValue<JsonString>(Component.Discriminator, out var type) && field.TryGetResolvedSchema(type.Value, out var schema))
            {
                foreach (var componentField in schema.Fields)
                {
                    if (obj.TryGetValue(componentField.Name, out var componentValue))
                    {
                        componentField.Accept(this, new Args(componentValue, args.Result, args.ResultLimit));
                    }
                }
            }
        }

        private static void AddIds(ref Args args)
        {
            var added = 0;

            if (args.Value is JsonArray array)
            {
                foreach (var id in array)
                {
                    if (id is JsonString s)
                    {
                        args.Result.Add(DomainId.Create(s.Value));

                        added++;

                        if (added >= args.ResultLimit)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
