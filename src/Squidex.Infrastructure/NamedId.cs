// ==========================================================================
//  NamedId.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
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
            return other != null && (ReferenceEquals(this, other) || Id.Equals(other.Id)));
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
