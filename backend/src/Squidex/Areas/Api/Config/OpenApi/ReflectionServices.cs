// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Namotion.Reflection;
using NJsonSchema.Generation;
using Squidex.Assets;
using Squidex.Infrastructure.Collections;
using System.Diagnostics;

namespace Squidex.Areas.Api.Config.OpenApi;

public class ReflectionServices : SystemTextJsonReflectionService
{
    private static HashSet<string> written = [];

    protected override JsonTypeDescription GetDescription(ContextualType contextualType, SystemTextJsonSchemaGeneratorSettings settings, Type originalType, bool isNullable, ReferenceTypeNullHandling defaultReferenceTypeNullHandling)
    {
        if (contextualType.Type == typeof(IAssetFile))
        {
            Debugger.Break();
        }

        return base.GetDescription(contextualType, settings, originalType, isNullable, defaultReferenceTypeNullHandling);
    }

    protected override bool IsBinary(ContextualType contextualType)
    {
        var parameterTypeName = contextualType.Name;
        if (parameterTypeName.Contains("Asset") && written.Add(parameterTypeName))
        {

            Console.WriteLine(parameterTypeName);
        }

        if (parameterTypeName.Contains("CreateAssetDto"))
        {

        }

        if (contextualType.Type == typeof(IAssetFile))
        {
            return true;
        }

        var x = base.IsBinary(contextualType);

        if (x)
        {
            Debugger.Break();
        }

        return x;
    }

    protected override bool IsArrayType(ContextualType contextualType)
    {
        if (contextualType.Type.IsGenericType &&
            contextualType.Type.GetGenericTypeDefinition() == typeof(ReadonlyList<>))
        {
            return true;
        }

        return base.IsArrayType(contextualType);
    }

    protected override bool IsDictionaryType(ContextualType contextualType)
    {
        if (contextualType.Type.IsGenericType &&
            contextualType.Type.GetGenericTypeDefinition() == typeof(ReadonlyDictionary<,>))
        {
            return true;
        }

        return base.IsDictionaryType(contextualType);
    }
}
