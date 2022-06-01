// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NodaTime;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

namespace Squidex.Infrastructure.Json.Objects
{
    public readonly struct JsonValue2 : IEquatable<JsonValue2>
    {
        private static readonly char[] PathSeparators = { '.', '[', ']' };

        private readonly object? value;

        public object? RawValue => value;

        public JsonValueType Type
        {
            get
            {
                switch (value)
                {
                    case null:
                        return JsonValueType.Null;
                    case bool:
                        return JsonValueType.Boolean;
                    case double:
                        return JsonValueType.Number;
                    case string:
                        return JsonValueType.String;
                    case JsonArray:
                        return JsonValueType.Array;
                    case JsonObject:
                        return JsonValueType.Object;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public bool AsBoolean
        {
            get
            {
                if (value is bool typed)
                {
                    return typed;
                }

                throw new InvalidOperationException("Not a boolean.");
            }
        }

        public double AsNumber
        {
            get
            {
                if (value is double typed)
                {
                    return typed;
                }

                throw new InvalidOperationException("Not a number.");
            }
        }

        public string AsString
        {
            get
            {
                if (value is string typed)
                {
                    return typed;
                }

                throw new InvalidOperationException("Not a string.");
            }
        }

        public JsonArray AsArray
        {
            get
            {
                if (value is JsonArray typed)
                {
                    return typed;
                }

                throw new InvalidOperationException("Not an array.");
            }
        }

        public JsonObject AsObject
        {
            get
            {
                if (value is JsonObject typed)
                {
                    return typed;
                }

                throw new InvalidOperationException("Not an object.");
            }
        }

        public JsonValue2(double value)
        {
            Guard.ValidNumber(value);

            this.value = value;
        }

        public JsonValue2(bool value)
        {
            this.value = value;
        }

        public JsonValue2(string? value)
        {
            this.value = value;
        }

        public JsonValue2(JsonArray? value)
        {
            this.value = value;
        }

        public JsonValue2(JsonObject? value)
        {
            this.value = value;
        }

        public static JsonValue2 Create<T>(IReadOnlyDictionary<string, T>? values)
        {
            var source = new JsonObject(values?.Count ?? 0);

            if (values != null)
            {
                foreach (var (key, value) in values)
                {
                    source[key] = Create(value);
                }
            }

            return source;
        }

        public static JsonValue2 Create(object? value)
        {
            if (value == null)
            {
                return default;
            }

            if (value is JsonValue2 v)
            {
                return v;
            }

            switch (value)
            {
                case Guid typed:
                    return Create(typed.ToString());
                case DomainId typed:
                    return Create(typed);
                case Instant typed:
                    return Create(typed);
                case bool typed:
                    return Create(typed);
                case float typed:
                    return Create(typed);
                case double typed:
                    return Create(typed);
                case int typed:
                    return Create(typed);
                case long typed:
                    return Create(typed);
                case string typed:
                    return Create(typed);
                case object[] typed:
                    return Create(typed);
                case IReadOnlyDictionary<string, object?> typed:
                    return Create(typed);
            }

            throw new ArgumentException("Invalid json type", nameof(value));
        }

        public static JsonValue2 Object()
        {
            return new JsonValue2(new JsonObject());
        }

        public static JsonValue2 Array()
        {
            return new JsonValue2(new JsonArray());
        }

        public static JsonValue2 Create<T>(IEnumerable<T> values)
        {
            var source = new JsonArray(values?.OfType<object?>().Select(Create));

            return new JsonValue2(source);
        }

        public static JsonValue2 Create<T>(params T?[] values)
        {
            var source = new JsonArray(values?.OfType<object?>().Select(Create));

            return new JsonValue2(source);
        }

        public static JsonValue2 Create(DomainId value)
        {
            return new JsonValue2(value.ToString());
        }

        public static JsonValue2 Create(Instant value)
        {
            return new JsonValue2(value.ToString());
        }

        public static JsonValue2 Create(double value)
        {
            return new JsonValue2(value);
        }

        public static JsonValue2 Create(bool value)
        {
            return new JsonValue2(value);
        }

        public static JsonValue2 Create(string? value)
        {
            return new JsonValue2(value);
        }

        public static JsonValue2 Create(JsonArray? array)
        {
            return new JsonValue2(array);
        }

        public static JsonValue2 Create(JsonObject? @object)
        {
            return new JsonValue2(@object);
        }

        public static implicit operator JsonValue2(DomainId value)
        {
            return Create(value);
        }

        public static implicit operator JsonValue2(Instant value)
        {
            return Create(value);
        }

        public static implicit operator JsonValue2(bool value)
        {
            return Create(value);
        }

        public static implicit operator JsonValue2(double value)
        {
            return Create(value);
        }

        public static implicit operator JsonValue2(string? value)
        {
            return Create(value);
        }

        public static implicit operator JsonValue2(JsonArray value)
        {
            return Create(value);
        }

        public static implicit operator JsonValue2(JsonObject value)
        {
            return Create(value);
        }

        public static bool operator ==(JsonValue2 left, JsonValue2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(JsonValue2 left, JsonValue2 right)
        {
            return !(left == right);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is JsonValue2 typed && Equals(typed);
        }

        public bool Equals(JsonValue2 other)
        {
            if (other.Type != Type)
            {
                return false;
            }

            switch (value)
            {
                case null:
                    return true;
                case bool b:
                    return b == (bool)other.value!;
                case double d:
                    return d == (double)other.value!;
                case string s:
                    return s == (string)other.value!;
                case JsonArray a:
                    return a.Equals((JsonArray)other.value!);
                case JsonObject o:
                    return o.Equals((JsonObject)other.value!);
                default:
                    throw new InvalidOperationException();
            }
        }

        public override int GetHashCode()
        {
            switch (value)
            {
                case null:
                    return 0;
                case bool b:
                    return b.GetHashCode();
                case double d:
                    return d.GetHashCode();
                case string s:
                    return s.GetHashCode(StringComparison.OrdinalIgnoreCase);
                case JsonArray a:
                    return a.GetHashCode();
                case JsonObject o:
                    return o.GetHashCode();
                default:
                    throw new InvalidOperationException();
            }
        }

        public override string ToString()
        {
            switch (value)
            {
                case null:
                    return "null";
                case bool b:
                    return b ? "true" : "false";
                case double d:
                    return d.ToString(CultureInfo.InvariantCulture);
                case string s:
                    return s;
                case JsonArray a:
                    return a.ToString();
                case JsonObject o:
                    return o.ToString();
                default:
                    throw new InvalidOperationException();
            }
        }

        public string ToJsonString()
        {
            switch (value)
            {
                case null:
                    return "null";
                case bool b:
                    return b ? "true" : "false";
                case double d:
                    return d.ToString(CultureInfo.InvariantCulture);
                case string s:
                    return $"\"{s}\"";
                case JsonArray a:
                    return a.ToJsonString();
                case JsonObject o:
                    return o.ToJsonString();
                default:
                    throw new InvalidOperationException();
            }
        }

        public JsonValue2 Clone()
        {
            switch (value)
            {
                case null:
                    return this;
                case bool b:
                    return this;
                case double d:
                    return this;
                case string s:
                    return this;
                case JsonArray a:
                    return new JsonValue2(new JsonArray(a.Select(x => x.Clone())));
                case JsonObject o:
                    return this;
                default:
                    throw new InvalidOperationException();
            }
        }

        public bool TryGetByPath(string? path, [MaybeNullWhen(false)] out JsonValue2 result)
        {
            return TryGetByPath(path?.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries), out result!);
        }

        public bool TryGetByPath(IEnumerable<string>? path, [MaybeNullWhen(false)] out JsonValue2 result)
        {
            result = this!;

            if (path == null)
            {
                return false;
            }

            foreach (var pathSegment in path)
            {
                if (!result.TryGetValue(pathSegment, out result!))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryGetValue(JsonValueType type, string pathSegment, [MaybeNullWhen(false)] out JsonValue2 result)
        {
            result = default!;

            if (TryGetValue(pathSegment, out var temp) && result.Type == type)
            {
                result = temp;
                return true;
            }

            return false;
        }

        public bool TryGetValue(string pathSegment, [MaybeNullWhen(false)] out JsonValue2 result)
        {
            result = default!;

            if (pathSegment == null)
            {
                return false;
            }

            switch (value)
            {
                case null:
                    return false;
                case bool:
                    return false;
                case double:
                    return false;
                case string:
                    return false;
                case JsonArray a:
                    return a.TryGetValue(pathSegment, out result);
                case JsonObject o:
                    return o.TryGetValue(pathSegment, out result);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
