// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NodaTime;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

namespace Squidex.Infrastructure.Json.Objects
{
    public static class JsonValue
    {
        private static readonly char[] PathSeparators = { '.', '[', ']' };

        public static readonly IJsonValue Empty = new JsonString(string.Empty);

        public static readonly IJsonValue True = JsonBoolean.True;
        public static readonly IJsonValue False = JsonBoolean.False;

        public static readonly IJsonValue Null = JsonNull.Null;

        public static readonly IJsonValue Zero = new JsonNumber(0);

        public static JsonArray Array()
        {
            return new JsonArray();
        }

        public static JsonArray Array<T>(IEnumerable<T> values)
        {
            var source = values?.OfType<object?>().Select(Create).ToList() ?? new List<IJsonValue>();

            return new JsonArray(source);
        }

        public static JsonArray Array<T>(params T?[] values)
        {
            var source = values?.OfType<object?>().Select(Create).ToList() ?? new List<IJsonValue>();

            return new JsonArray(source);
        }

        public static JsonObject Object()
        {
            return new JsonObject();
        }

        public static JsonObject Object<T>(IReadOnlyDictionary<string, T> values)
        {
            var source = values?.ToDictionary(x => x.Key, x => Create(x.Value)) ?? new Dictionary<string, IJsonValue>();

            return new JsonObject(source);
        }

        public static IJsonValue Create(object? value)
        {
            if (value == null)
            {
                return Null;
            }

            if (value is IJsonValue v)
            {
                return v;
            }

            switch (value)
            {
                case string s:
                    return Create(s);
                case bool b:
                    return Create(b);
                case float f:
                    return Create(f);
                case double d:
                    return Create(d);
                case int i:
                    return Create(i);
                case long l:
                    return Create(l);
                case Guid g:
                    return Create(g);
                case DomainId i:
                    return Create(i);
                case Instant i:
                    return Create(i);
                case object[] array:
                    return Array(array);
                case IReadOnlyDictionary<string, object?> obj:
                    return Object(obj);
            }

            throw new ArgumentException("Invalid json type", nameof(value));
        }

        public static IJsonValue Create(Guid value)
        {
            return Create(value.ToString());
        }

        public static IJsonValue Create(DomainId value)
        {
            return Create(value.ToString());
        }

        public static IJsonValue Create(Guid? value)
        {
            if (value == null)
            {
                return Null;
            }

            return Create(value.Value);
        }

        public static IJsonValue Create(Instant value)
        {
            return Create(value.ToString());
        }

        public static IJsonValue Create(Instant? value)
        {
            if (value == null)
            {
                return Null;
            }

            return Create(value.Value);
        }

        public static IJsonValue Create(double value)
        {
            Guard.ValidNumber(value, nameof(value));

            if (value == 0)
            {
                return Zero;
            }

            return new JsonNumber(value);
        }

        public static IJsonValue Create(double? value)
        {
            if (value == null)
            {
                return Null;
            }

            return Create(value.Value);
        }

        public static IJsonValue Create(bool value)
        {
            return value ? True : False;
        }

        public static IJsonValue Create(bool? value)
        {
            if (value == null)
            {
                return Null;
            }

            return Create(value.Value);
        }

        public static IJsonValue Create(string? value)
        {
            if (value == null)
            {
                return Null;
            }

            if (value.Length == 0)
            {
                return Empty;
            }

            return new JsonString(value);
        }

        public static bool TryGetByPath(this IJsonValue value, string? path, [MaybeNullWhen(false)] out IJsonValue result)
        {
            return TryGetByPath(value, path?.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries), out result!);
        }

        public static bool TryGetByPath(this IJsonValue? value, IEnumerable<string>? path, [MaybeNullWhen(false)] out IJsonValue result)
        {
            result = value!;

            if (path != null)
            {
                foreach (var pathSegment in path)
                {
                    if (result == null || !result.TryGet(pathSegment, out result!))
                    {
                        break;
                    }
                }
            }

            return result != null && !ReferenceEquals(result, value);
        }
    }
}
