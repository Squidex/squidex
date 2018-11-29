// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

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

        public static bool TryParse(string value, out RefToken result)
        {
            if (value != null)
            {
                var idx = value.IndexOf(':');

                if (idx > 0 && idx < value.Length - 1)
                {
                    result = new RefToken(value.Substring(0, idx), value.Substring(idx + 1));

                    return true;
                }
            }

            result = null;

            return false;
        }

        public static RefToken Parse(string value)
        {
            if (!TryParse(value, out var result))
            {
                throw new ArgumentException("Ref token must have more than 2 parts divided by colon.", nameof(value));
            }

            return result;
        }
    }
}
