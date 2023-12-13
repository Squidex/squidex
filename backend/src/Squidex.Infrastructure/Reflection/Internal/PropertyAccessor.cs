// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Reflection.Emit;

namespace Squidex.Infrastructure.Reflection.Internal;

public static class PropertyAccessor
{
    public delegate TValue Getter<TSource, TValue>(TSource source);

    public delegate void Setter<TSource, TValue>(TSource source, TValue value);

    public static Getter<TSource, TValue> CreateGetter<TSource, TValue>(PropertyInfo propertyInfo)
    {
        if (!propertyInfo.CanRead)
        {
            return x => throw new NotSupportedException();
        }

        var bakingField =
            propertyInfo.DeclaringType!.GetField($"<{propertyInfo.Name}>k__BackingField",
                BindingFlags.NonPublic |
                BindingFlags.Instance);

        var propertyGetMethod = propertyInfo.GetGetMethod()!;

        var getMethod = new DynamicMethod(propertyGetMethod.Name, typeof(TValue), [typeof(TSource)], true);
        var getGenerator = getMethod.GetILGenerator();

        // Load this to stack.
        getGenerator.Emit(OpCodes.Ldarg_0);

        if (bakingField != null && !propertyGetMethod.IsVirtual)
        {
            // Get field directly.
            getGenerator.Emit(OpCodes.Ldfld, bakingField);
        }
        else if (propertyGetMethod.IsVirtual)
        {
            // Call the virtual property.
            getGenerator.Emit(OpCodes.Callvirt, propertyGetMethod);
        }
        else
        {
            // Call the non virtual property.
            getGenerator.Emit(OpCodes.Call, propertyGetMethod);
        }

        getGenerator.Emit(OpCodes.Ret);

        return getMethod.CreateDelegate<Getter<TSource, TValue>>();
    }

    public static Setter<TSource, TValue> CreateSetter<TSource, TValue>(PropertyInfo propertyInfo)
    {
        if (!propertyInfo.CanWrite)
        {
            return (x, y) => throw new NotSupportedException();
        }

        var bakingField =
            propertyInfo.DeclaringType!.GetField($"<{propertyInfo.Name}>k__BackingField",
                BindingFlags.NonPublic |
                BindingFlags.Instance);

        var propertySetMethod = propertyInfo.GetSetMethod()!;

        var setMethod = new DynamicMethod(propertySetMethod.Name, null, [typeof(TSource), typeof(TValue)], true);
        var setGenerator = setMethod.GetILGenerator();

        // Load this to stack.
        setGenerator.Emit(OpCodes.Ldarg_0);

        // Load argument to stack.
        setGenerator.Emit(OpCodes.Ldarg_1);

        if (bakingField != null && !propertySetMethod.IsVirtual)
        {
            // Set the baking field directly.
            setGenerator.Emit(OpCodes.Stfld, bakingField);
        }
        else if (propertySetMethod.IsVirtual)
        {
            // Call the virtual property.
            setGenerator.Emit(OpCodes.Callvirt, propertySetMethod);
        }
        else
        {
            // Call the non virtual property.
            setGenerator.Emit(OpCodes.Call, propertySetMethod);
        }

        setGenerator.Emit(OpCodes.Ret);

        return setMethod.CreateDelegate<Setter<TSource, TValue>>();
    }
}
