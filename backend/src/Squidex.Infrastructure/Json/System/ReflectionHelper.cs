// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Reflection.Emit;

namespace Squidex.Infrastructure.Json.System;

internal static class ReflectionHelper
{
    public static Func<TInput, TInstance> CreateParameterizedConstructor<TInstance, TInput>()
    {
        var method = CreateParameterizedConstructor(typeof(TInstance), typeof(TInput));

        return method.CreateDelegate<Func<TInput, TInstance>>();
    }

    private static DynamicMethod CreateParameterizedConstructor(Type type, Type parameterType)
    {
        var constructor =
            type.GetConstructors()
                .Single(x =>
                    x.GetParameters().Length == 1 &&
                    x.GetParameters()[0].ParameterType == parameterType);

        var dynamicMethod = new DynamicMethod(
            ConstructorInfo.ConstructorName,
            type,
            new[] { parameterType },
            typeof(ReflectionHelper).Module,
            true);

        var generator = dynamicMethod.GetILGenerator();

        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Newobj, constructor);
        generator.Emit(OpCodes.Ret);

        return dynamicMethod;
    }

    public static string GetShortTypeName(this Type type)
    {
        return $"{type.FullName}, {type.Assembly.GetName().Name}";
    }
}
