// ==========================================================================
//  EnumExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

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
    }
}
