// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Security
{
    public sealed partial class Permission
    {
        internal struct Part
        {
            private static readonly char[] AlternativeSeparators = { '|' };
            private static readonly char[] MainSeparators = { '.' };

            public readonly HashSet<string> Alternatives;

            public readonly bool Exclusion;

            public Part(HashSet<string> alternatives, bool exclusion)
            {
                Alternatives = alternatives;

                Exclusion = exclusion;
            }

            public static Part[] ParsePath(string path)
            {
                return path
                    .Split(MainSeparators, StringSplitOptions.RemoveEmptyEntries)
                    .Select(Parse)
                    .ToArray();
            }

            public static Part Parse(string part)
            {
                var isExclusion = false;

                if (part.StartsWith(Exclude, StringComparison.OrdinalIgnoreCase))
                {
                    isExclusion = true;

                    part = part.Substring(1);
                }

                HashSet<string> alternatives = null;

                if (part != Any)
                {
                    alternatives =
                        part.Split(AlternativeSeparators, StringSplitOptions.RemoveEmptyEntries)
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);
                }

                return new Part(alternatives, isExclusion);
            }

            public static bool Intersects(ref Part lhs, ref Part rhs, bool allowNull)
            {
                if (lhs.Alternatives == null)
                {
                    return true;
                }

                if (allowNull && rhs.Alternatives == null)
                {
                    return true;
                }

                bool shouldIntersect = !(lhs.Exclusion ^ rhs.Exclusion);

                return rhs.Alternatives != null && lhs.Alternatives.Intersect(rhs.Alternatives).Any() == shouldIntersect;
            }
        }
    }
}
