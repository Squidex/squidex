// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure
{
    public delegate bool Parser<T>(ReadOnlySpan<char> input, out T result);

    [TypeConverter(typeof(NamedIdTypeConverter))]
    public sealed record NamedId<T> where T : notnull
    {
        private static readonly int GuidLength = Guid.Empty.ToString().Length;

        public T Id { get; }

        public string Name { get; }

        public NamedId(T id, string name)
        {
            Guard.NotNull(id, nameof(id));
            Guard.NotNull(name, nameof(name));

            Id = id;

            Name = name;
        }

        public override string ToString()
        {
            return $"{Id},{Name}";
        }

        public static bool TryParse(string value, Parser<T> parser, [MaybeNullWhen(false)] out NamedId<T> result)
        {
            if (value != null)
            {
                var span = value.AsSpan();

                if (typeof(T) == typeof(Guid))
                {
                    if (value.Length > GuidLength + 1 && value[GuidLength] == ',')
                    {
                        if (parser(span.Slice(0, GuidLength), out var id))
                        {
                            result = new NamedId<T>(id, value[(GuidLength + 1)..]);

                            return true;
                        }
                    }
                }
                else
                {
                    var index = value.IndexOf(',', StringComparison.Ordinal);

                    if (index > 0 && index < value.Length - 1)
                    {
                        if (parser(span.Slice(0, index), out var id))
                        {
                            result = new NamedId<T>(id, value[(index + 1)..]);

                            return true;
                        }
                    }
                }
            }

            result = null!;

            return false;
        }

        public static NamedId<T> Parse(string value, Parser<T> parser)
        {
            if (!TryParse(value, parser, out var result))
            {
                throw new ArgumentException("Named id must have at least 2 parts divided by commata.", nameof(value));
            }

            return result;
        }
    }
}
