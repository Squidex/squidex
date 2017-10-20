// ==========================================================================
//  RefToken.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;

namespace Squidex.Infrastructure
{
    public sealed class RefToken : IEquatable<RefToken>
    {
        public string Type { get; }

        public string Identifier { get; }

        public RefToken(string type, string identifier)
        {
            Guard.NotNullOrEmpty(type, nameof(type));
            Guard.NotNullOrEmpty(identifier, nameof(identifier));

            Type = type.ToLowerInvariant();

            Identifier = identifier;
        }

        public static RefToken Parse(string input)
        {
            Guard.NotNullOrEmpty(input, nameof(input));

            var parts = input.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new ArgumentException("Input must have more than 2 parts divided by colon.", nameof(input));
            }

            return new RefToken(parts[0], string.Join(":", parts.Skip(1)));
        }

        public override string ToString()
        {
            return $"{Type}:{Identifier}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RefToken);
        }

        public bool Equals(RefToken other)
        {
            return other != null && (ReferenceEquals(this, other) || (Type.Equals(other.Type) && Identifier.Equals(other.Identifier)));
        }

        public override int GetHashCode()
        {
            return (Type.GetHashCode() * 397) ^ Identifier.GetHashCode();
        }
    }
}
