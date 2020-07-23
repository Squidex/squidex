// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure.Validation
{
    public static class Not
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Defined()
        {
            return T.Get("validation.requiredValue");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Defined(string property)
        {
            return T.Get("validation.required", new { property });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Defined2(string property1, string property2)
        {
            return T.Get("validation.requiredBoth", new { property1, property2 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ValidSlug(string property)
        {
            return T.Get("validation.slug", new { property });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ValidPropertyName(string property)
        {
            return T.Get("validation.javascriptProperty", new { property });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GreaterThan(string property, string other)
        {
            return T.Get("validation.greaterThan", new { property, other });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GreaterEqualsThan(string property, string other)
        {
            return T.Get("validation.greaterEqualsThan", new { property, other });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string LessOther(string property, string other)
        {
            return T.Get("validation.lessThan", new { property, other });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string LessEqualsOther(string property, string other)
        {
            return T.Get("validation.lessEqualsThan", new { property, other });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Between<TField>(string property, TField min, TField max)
        {
            return T.Get("validation.between", new { property, min, max });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Valid(string property)
        {
            return T.Get("validation.valid", new { property });
        }
    }
}
