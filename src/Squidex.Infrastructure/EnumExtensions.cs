// ==========================================================================
//  EnumExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text.RegularExpressions;

// ReSharper disable ObjectCreationAsStatement

namespace Squidex.Infrastructure
{
    public static class EnumExtensions
    {
        public static bool IsEnumValue<TEnum>(this TEnum value) where TEnum : struct
        {
            try
            {
                return Enum.IsDefined(typeof(TEnum), value);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidRegex(this string value)
        {
            try
            {
                new Regex(value);

                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
