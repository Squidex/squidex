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
    [TypeConverter(typeof(RefTokenTypeConverter))]
    public sealed record RefToken
    {
        private static readonly char[] TrimChars = { ' ', ':' };

        public RefTokenType Type { get; }

        public string Identifier { get; }

        public bool IsClient
        {
            get => Type == RefTokenType.Client;
        }

        public bool IsUser
        {
            get => Type == RefTokenType.Subject;
        }

        public RefToken(RefTokenType type, string identifier)
        {
            Guard.NotNullOrEmpty(identifier, nameof(identifier));

            Type = type;

            Identifier = identifier;
        }

        public static RefToken Client(string identifier)
        {
            return new RefToken(RefTokenType.Client, identifier);
        }

        public static RefToken User(string identifier)
        {
            return new RefToken(RefTokenType.Subject, identifier);
        }

        public override string ToString()
        {
            return $"{Type.ToString().ToLowerInvariant()}:{Identifier}";
        }

        public static bool TryParse(string? value, [MaybeNullWhen(false)] out RefToken result)
        {
            value = value?.Trim(TrimChars);

            if (string.IsNullOrWhiteSpace(value))
            {
                result = null!;
                return false;
            }

            value = value.Trim();

            var idx = value.IndexOf(':', StringComparison.Ordinal);

            if (idx > 0 && idx < value.Length - 1)
            {
                if (!Enum.TryParse<RefTokenType>(value.Substring(0, idx), true, out var type))
                {
                    type = RefTokenType.Subject;
                }

                result = new RefToken(type, value[(idx + 1)..]);
            }
            else
            {
                result = new RefToken(RefTokenType.Subject, value);
            }

            return true;
        }

        public static RefToken Parse(string value)
        {
            if (!TryParse(value, out var result))
            {
                throw new ArgumentException("Ref token cannot be null or empty.", nameof(value));
            }

            return result;
        }
    }
}
