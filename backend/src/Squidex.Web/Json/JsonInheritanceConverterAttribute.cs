// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;

namespace Squidex.Web.Json;

public sealed class JsonInheritanceConverterAttribute : JsonConverterAttribute
{
    public string DiscriminatorName { get; }

    public JsonInheritanceConverterAttribute(Type baseType, string discriminatorName = "$type")
        : base(typeof(JsonInheritanceConverter<>).MakeGenericType(baseType))
    {
        DiscriminatorName = discriminatorName;
    }
}
