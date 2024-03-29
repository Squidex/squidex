﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Namotion.Reflection;
using NJsonSchema.Generation;
using Squidex.Infrastructure.Collections;

namespace Squidex.Areas.Api.Config.OpenApi;

public class ReflectionServices : SystemTextJsonReflectionService
{
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
