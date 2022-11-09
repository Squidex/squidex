// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;

namespace Squidex.Infrastructure.Reflection.Internal;

public sealed class PropertyAccessor
{
    private interface IPropertyAccessor
    {
        object? Get(object target);

        void Set(object target, object? value);
    }

    private sealed class PropertyWrapper<TObject, TValue> : IPropertyAccessor
    {
        private readonly Func<TObject, TValue> getMethod;
        private readonly Action<TObject, TValue> setMethod;

        public PropertyWrapper(PropertyInfo propertyInfo)
        {
            if (propertyInfo.CanRead)
            {
                getMethod = (Func<TObject, TValue>)propertyInfo.GetGetMethod(true)!.CreateDelegate(typeof(Func<TObject, TValue>));
            }
            else
            {
                getMethod = x => throw new NotSupportedException();
            }

            if (propertyInfo.CanWrite)
            {
                setMethod = (Action<TObject, TValue>)propertyInfo.GetSetMethod(true)!.CreateDelegate(typeof(Action<TObject, TValue>));
            }
            else
            {
                setMethod = (x, y) => throw new NotSupportedException();
            }
        }

        public object? Get(object source)
        {
            return getMethod((TObject)source);
        }

        public void Set(object source, object? value)
        {
            setMethod((TObject)source, (TValue)value!);
        }
    }

    private readonly IPropertyAccessor internalAccessor;

    public PropertyAccessor(PropertyInfo propertyInfo)
    {
        Guard.NotNull(propertyInfo);

        var type = typeof(PropertyWrapper<,>).MakeGenericType(propertyInfo.DeclaringType!, propertyInfo.PropertyType);

        internalAccessor = (IPropertyAccessor)Activator.CreateInstance(type, propertyInfo)!;
    }

    public object? Get(object target)
    {
        Guard.NotNull(target);

        return internalAccessor.Get(target);
    }

    public void Set(object target, object? value)
    {
        Guard.NotNull(target);

        internalAccessor.Set(target, value);
    }
}
