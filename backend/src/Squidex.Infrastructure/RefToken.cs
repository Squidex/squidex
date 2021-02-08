// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure
{
    [TypeConverter(typeof(RefTokenTypeConverter))]
    public sealed record RefToken
    {
        public RefTokenType Type { get; }

        public string Identifier { get; }

        public bool IsClient
        {
            get { return Type == RefTokenType.Client; }
        }

        public bool IsSubject
        {
            get { return Type == RefTokenType.Subject; }
        }

        public RefToken(RefTokenType type, string identifier)
        {
            Guard.NotNullOrEmpty(identifier, nameof(identifier));

            Type = type;

            Identifier = identifier;
        }

        public override string ToString()
        {
            return $"{Type.ToString().ToLowerInvariant()}:{Identifier}";
        }

        public override int GetHashCode()
        {
            return (Type.GetHashCode() * 397) ^ Identifier.GetHashCode();
        }

        public static bool TryParse(string value, [MaybeNullWhen(false)] out RefToken result)
        {
            result = null!;

            if (value != null)
            {
                var idx = value.IndexOf(':');

                if (idx > 0 && idx < value.Length - 1)
                {
                    if (Enum.TryParse<RefTokenType>(value.Substring(0, idx), true, out var type))
                    {
                        result = new RefToken(type, value[(idx + 1)..]);
                        return true;
                    }
                }
            }

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
