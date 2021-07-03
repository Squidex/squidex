// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    internal sealed class ReferencesCleaner : IFieldVisitor<IJsonValue, ReferencesCleaner.Args>
    {
        private static readonly ReferencesCleaner Instance = new ReferencesCleaner();

        public sealed record Args(IJsonValue Value, ISet<DomainId> ValidIds);

        private ReferencesCleaner()
        {
        }

        public static IJsonValue Cleanup(IField field, IJsonValue? value, HashSet<DomainId> validIds)
        {
            var args = new Args(value ?? JsonValue.Null, validIds);

            return field.Accept(Instance, args);
        }

        public IJsonValue Visit(IField<AssetsFieldProperties> field, Args args)
        {
            return CleanIds(args);
        }

        public IJsonValue Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            return CleanIds(args);
        }

        public IJsonValue Visit(IField<BooleanFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IField<ComponentFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IField<ComponentsFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IField<JsonFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IField<NumberFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IField<StringFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IField<TagsFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IField<UIFieldProperties> field, Args args)
        {
            return args.Value;
        }

        public IJsonValue Visit(IArrayField field, Args args)
        {
            return args.Value;
        }

        private static IJsonValue CleanIds(Args args)
        {
            if (args.Value is JsonArray array)
            {
                var result = array;

                for (var i = 0; i < result.Count; i++)
                {
                    if (!IsValidReference(result[i], args))
                    {
                        if (ReferenceEquals(result, array))
                        {
                            result = new JsonArray(array);
                        }

                        result.RemoveAt(i);
                        i--;
                    }
                }

                return result;
            }

            return args.Value;
        }

        private static bool IsValidReference(IJsonValue item, Args args)
        {
            return item is JsonString s && args.ValidIds.Contains(DomainId.Create(s.Value));
        }
    }
}
