// ==========================================================================
//  EnumExtensions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure
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
