// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;

namespace Squidex.Infrastructure.Security
{
    public sealed partial class Permission
    {
        internal struct Part
        {
            private static readonly char[] AlternativeSeparators = { '|' };
            private static readonly char[] MainSeparators = { '.' };

            public readonly string[] Alternatives;

            public readonly bool Exclusion;

            public Part(string[] alternatives, bool exclusion)
            {
                Alternatives = alternatives;

                Exclusion = exclusion;
            }

            public static Part[] ParsePath(string path)
            {
                var parts = path.Split(MainSeparators, StringSplitOptions.RemoveEmptyEntries);

                var result = new Part[parts.Length];

                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = Parse(parts[i]);
                }

                return result;
            }

            public static Part Parse(string part)
            {
                var isExclusion = false;

                if (part.StartsWith(Exclude, StringComparison.OrdinalIgnoreCase))
                {
                    isExclusion = true;

                    part = part.Substring(1);
                }

                string[] alternatives = null;

                if (part != Any)
                {
                    alternatives = part.Split(AlternativeSeparators, StringSplitOptions.RemoveEmptyEntries);
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

                var shouldIntersect = !(lhs.Exclusion ^ rhs.Exclusion);

                return rhs.Alternatives != null && lhs.Alternatives.Intersect(rhs.Alternatives).Any() == shouldIntersect;
            }
        }
    }
}
