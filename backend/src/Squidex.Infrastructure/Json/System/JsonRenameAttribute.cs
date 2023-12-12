// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace Squidex.Infrastructure.Json.System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class JsonRenameAttribute(string propertyName, string jsonName) : Attribute
{
    public string PropertyName { get; } = propertyName;

    public string JsonName { get; } = jsonName;

    public static void Modifier(JsonTypeInfo typeInfo)
    {
        var attributes = typeInfo.Type.GetCustomAttributes<JsonRenameAttribute>();

        foreach (var property in typeInfo.Properties)
        {
            var memberName = (property.AttributeProvider as MemberInfo)?.Name;

            var attribute = attributes.FirstOrDefault(x => x.PropertyName == memberName);
            if (attribute != null)
            {
                property.Name = attribute.JsonName;
            }
        }
    }
}
