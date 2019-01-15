// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;

namespace Squidex.Infrastructure
{
    public static class Not
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Defined(string property)
        {
            return $"{Upper(property)} is required.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Defined2(string property1, string property2)
        {
            return $"If {Lower(property1)} or {Lower(property2)} is used both must be defined.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ValidSlug(string property)
        {
            return $"{Upper(property)} is not a valid slug.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GreaterThan(string property, string other)
        {
            return $"{Upper(property)} must be greater than {Lower(other)}.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GreaterEquals(string property, string other)
        {
            return $"{Upper(property)} must be greater or equal to {Lower(other)}.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string LessThan(string property, string other)
        {
            return $"{Upper(property)} must be less than {Lower(other)}.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string LessEquals(string property, string other)
        {
            return $"{Upper(property)} must be less or equal to {Lower(other)}.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Between<T>(string property, T min, T max)
        {
            return $"{Upper(property)} must be between {min} and {max}.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Valid(string property)
        {
            return $"{Upper(property)} is not a valid value.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string New(string type, string property)
        {
            return $"{Upper(type)} has already this {Lower(property)}.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DefinedOr(string property1, string property2)
        {
            return $"Either {Lower(property1)} or {Lower(property2)} must be defined.";
        }

        private static string Lower(string property)
        {
            if (char.IsUpper(property[0]))
            {
                return char.ToLower(property[0]) + property.Substring(1);
            }

            return property;
        }

        private static string Upper(string property)
        {
            if (char.IsLower(property[0]))
            {
                return char.ToUpper(property[0]) + property.Substring(1);
            }

            return property;
        }
    }
}
