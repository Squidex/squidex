// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace Squidex.Infrastructure.Json.System;

public static class JsonIgnoreReadonlyProperties
{
    public static Action<JsonTypeInfo> Modifier<T>()
    {
        return new Action<JsonTypeInfo>(typeInfo =>
        {
            if (!typeInfo.Type.IsAssignableTo(typeof(T)))
            {
                return;
            }

            foreach (var property in typeInfo.Properties.ToList())
            {
                var memberInfo = property.AttributeProvider as PropertyInfo;

                if (memberInfo?.CanWrite == false)
                {
                    typeInfo.Properties.Remove(property);
                }
            }
        });
    }
}
