// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Security;

public sealed partial class Permission
{
    internal readonly struct Part
    {
        private const char SeparatorAlternative = '|';
        private const char SeparatorMain = '.';
        private const char CharAny = '*';
        private const char CharExclude = '^';

        public readonly ReadOnlyMemory<char>[]? Alternatives;

        public readonly bool Exclusion;

        public Part(ReadOnlyMemory<char>[]? alternatives, bool exclusion)
        {
            Alternatives = alternatives;

            Exclusion = exclusion;
        }

        public static Part[] ParsePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Array.Empty<Part>();
            }

            var current = path.AsMemory();
            var currentSpan = current.Span;

            var result = new Part[CountOf(currentSpan, SeparatorMain) + 1];

            if (result.Length == 1)
            {
                result[0] = Parse(current);
            }
            else
            {
                for (int i = 0, j = 0; i < currentSpan.Length; i++)
                {
                    if (currentSpan[i] == SeparatorMain)
                    {
                        result[j] = Parse(current[..i]);

                        current = current[(i + 1)..];
                        currentSpan = current.Span;

                        i = 0;
                        j++;
                    }
                    else if (i == currentSpan.Length - 1 || currentSpan[i] == SeparatorMain)
                    {
                        result[j] = Parse(current);
                    }
                }
            }

            return result;
        }

        public static Part Parse(ReadOnlyMemory<char> current)
        {
            var currentSpan = current.Span;

            if (currentSpan.Length == 0)
            {
                return new Part(Array.Empty<ReadOnlyMemory<char>>(), false);
            }

            var isExclusion = false;

            if (currentSpan[0] == CharExclude)
            {
                isExclusion = true;

                current = current[1..];
                currentSpan = current.Span;
            }

            if (currentSpan.Length == 0)
            {
                return new Part(Array.Empty<ReadOnlyMemory<char>>(), isExclusion);
            }

            if (current.Length > 1 || currentSpan[0] != CharAny)
            {
                var alternatives = new ReadOnlyMemory<char>[CountOf(currentSpan, SeparatorAlternative) + 1];

                if (alternatives.Length == 1)
                {
                    alternatives[0] = current;
                }
                else
                {
                    for (int i = 0, j = 0; i < current.Length; i++)
                    {
                        if (currentSpan[i] == SeparatorAlternative)
                        {
                            alternatives[j] = current[..i];

                            current = current[(i + 1)..];
                            currentSpan = current.Span;

                            i = 0;
                            j++;
                        }
                        else if (i == current.Length - 1)
                        {
                            alternatives[j] = current;
                        }
                    }
                }

                return new Part(alternatives, isExclusion);
            }
            else
            {
                return new Part(null, isExclusion);
            }
        }

        private static int CountOf(ReadOnlySpan<char> text, char character)
        {
            var length = text.Length;

            var count = 0;

            for (var i = 0; i < length; i++)
            {
                if (text[i] == character)
                {
                    count++;
                }
            }

            return count;
        }

        public static bool Intersects(ref Part lhs, ref Part rhs, bool allowNull)
        {
            if (lhs.Alternatives == null)
            {
                return true;
            }

            if (rhs.Alternatives == null)
            {
                return allowNull;
            }

            var shouldIntersect = !(lhs.Exclusion ^ rhs.Exclusion);

            var isIntersected = false;

            for (var i = 0; i < lhs.Alternatives.Length; i++)
            {
                for (var j = 0; j < rhs.Alternatives.Length; j++)
                {
                    if (lhs.Alternatives[i].Span.Equals(rhs.Alternatives[j].Span, StringComparison.OrdinalIgnoreCase))
                    {
                        isIntersected = true;
                        break;
                    }
                }
            }

            return isIntersected == shouldIntersect;
        }
    }
}
