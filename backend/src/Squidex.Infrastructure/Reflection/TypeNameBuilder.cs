// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Text;

namespace Squidex.Infrastructure.Reflection
{
    public static class TypeNameBuilder
    {
        public static string TypeName(this Type type, bool camelCase, params string[] suffixes)
        {
            var typeName = type.Name;

            if (suffixes != null)
            {
                foreach (var suffix in suffixes)
                {
                    if (typeName.EndsWith(suffix, StringComparison.Ordinal))
                    {
                        typeName = typeName.Substring(0, typeName.Length - suffix.Length);

                        break;
                    }
                }
            }

            return camelCase ? typeName.ToCamelCase() : typeName;
        }
    }
}
