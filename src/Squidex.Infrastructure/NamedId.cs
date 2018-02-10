// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;

namespace Squidex.Infrastructure
{
    public delegate bool Parser<T>(string input, out T result);

    public sealed class NamedId<T> : IEquatable<NamedId<T>>
    {
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

        public static NamedId<T> Parse(string value, Parser<T> parser)
        {
            Guard.NotNull(value, nameof(value));

            var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new ArgumentException("Named id must have more than 2 parts divided by commata.");
            }

            if (!parser(parts[0], out var id))
            {
                throw new ArgumentException("Named id must be a valid guid.");
            }

            return new NamedId<T>(id, string.Join(",", parts.Skip(1)));
        }
    }
}
