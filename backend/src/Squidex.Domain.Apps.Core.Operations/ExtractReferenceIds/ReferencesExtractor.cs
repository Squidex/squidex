// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    internal sealed class ReferencesExtractor : IFieldVisitor<None, ReferencesExtractor.Args>
    {
        private static readonly ReferencesExtractor Instance = new ReferencesExtractor();

        public record struct Args(JsonValue2 Value, ISet<DomainId> Result, int Take, ResolvedComponents Components);

        private ReferencesExtractor()
        {
        }

        public static None Extract(IField field, JsonValue2 value, HashSet<DomainId> result, int take, ResolvedComponents components)
        {
            var args = new Args(value, result, take, components);

            return field.Accept(Instance, args);
        }

        public None Visit(IArrayField field, Args args)
        {
            if (args.Value.Type == JsonValueType.Array)
            {
                foreach (var value in args.Value.AsArray)
                {
                    ExtractFromArrayItem(field, value, args);
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
            ExtractFromComponent(args.Value, args);

            return None.Value;
        }

        public None Visit(IField<ComponentsFieldProperties> field, Args args)
        {
            if (args.Value.Type == JsonValueType.Array)
            {
                foreach (var value in args.Value.AsArray)
                {
                    ExtractFromComponent(value, args);
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

        private void ExtractFromArrayItem(IArrayField field, JsonValue2 value, Args args)
        {
            if (value.Type == JsonValueType.Object)
            {
                var obj = value.AsObject;

                foreach (var nestedField in field.Fields)
                {
                    if (obj.TryGetValue(nestedField.Name, out var nestedValue))
                    {
                        nestedField.Accept(this, args with { Value = nestedValue });
                    }
                }
            }
        }

        private void ExtractFromComponent(JsonValue2 value, Args args)
        {
            if (value.Type == JsonValueType.Object)
            {
                var obj = value.AsObject;

                if (obj.TryGetValue(Component.Discriminator, out var type) && type.Type == JsonValueType.String)
                {
                    var id = DomainId.Create(type.AsString);

                    if (args.Components.TryGetValue(id, out var schema))
                    {
                        foreach (var componentField in schema.Fields)
                        {
                            if (obj.TryGetValue(componentField.Name, out var componentValue))
                            {
                                componentField.Accept(this, args with { Value = componentValue });
                            }
                        }
                    }
                }
            }
        }

        private static void AddIds(ref Args args)
        {
            var added = 0;

            if (args.Value.Type == JsonValueType.Array)
            {
                foreach (var id in args.Value.AsArray)
                {
                    if (id.Type == JsonValueType.String)
                    {
                        args.Result.Add(DomainId.Create(id.AsString));

                        added++;

                        if (added >= args.Take)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
