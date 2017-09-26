// ==========================================================================
//  EdmExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public static class EdmExtensions
    {
        public static string EscapeEdmField(this string field)
        {
            return field.Replace("-", "_");
        }

        public static string UnescapeEdmField(this string field)
        {
            return field.Replace("_", "-");
        }
    }
}
