// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure
{
    public delegate bool Parser<T>(string input, out T result);

    public sealed class NamedId<T> : IEquatable<NamedId<T>>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as NamedId<T>);
        }

        public bool Equals(NamedId<T> other)
        {
            return other != null && (ReferenceEquals(this, other) || (Id.Equals(other.Id) && Name.Equals(other.Name)));
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode() * 397) ^ Name.GetHashCode();
        }

        public static bool TryParse(string value, Parser<T> parser, out NamedId<T> result)
        {
            if (value != null)
            {
                if (typeof(T) == typeof(Guid))
                {
                    if (value.Length > GuidLength + 1 && value[GuidLength] == ',')
                    {
                        if (parser(value.Substring(0, GuidLength), out var id))
                        {
                            result = new NamedId<T>(id, value.Substring(GuidLength + 1));

                            return true;
                        }
                    }
                }
                else
                {
                    var index = value.IndexOf(',');

                    if (index > 0 && index < value.Length - 1)
                    {
                        if (parser(value.Substring(0, index), out var id))
                        {
                            result = new NamedId<T>(id, value.Substring(index + 1));

                            return true;
                        }
                    }
                }
            }

            result = null;

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
