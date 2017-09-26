// ==========================================================================
//  PropertyAccessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;

namespace Squidex.Infrastructure.Reflection
{
    public sealed class PropertyAccessor : IPropertyAccessor
    {
        private sealed class PropertyWrapper<TObject, TValue> : IPropertyAccessor
        {
            private readonly Func<TObject, TValue> getMethod;
            private readonly Action<TObject, TValue> setMethod;

            public PropertyWrapper(PropertyInfo propertyInfo)
            {
                if (propertyInfo.CanRead)
                {
                    getMethod = (Func<TObject, TValue>)propertyInfo.GetGetMethod(true).CreateDelegate(typeof(Func<TObject, TValue>));
                }
                else
                {
                    getMethod = x => throw new NotSupportedException();
                }

                if (propertyInfo.CanWrite)
                {
                    setMethod = (Action<TObject, TValue>)propertyInfo.GetSetMethod(true).CreateDelegate(typeof(Action<TObject, TValue>));
                }
                else
                {
                    setMethod = (x, y) => throw new NotSupportedException();
                }
            }

            public object Get(object source)
            {
                return getMethod((TObject)source);
            }

            public void Set(object source, object value)
            {
                setMethod((TObject)source, (TValue)value);
            }
        }

        private readonly IPropertyAccessor internalAccessor;

        public PropertyAccessor(Type targetType, PropertyInfo propertyInfo)
        {
            Guard.NotNull(targetType, nameof(targetType));
            Guard.NotNull(propertyInfo, nameof(propertyInfo));

            internalAccessor = (IPropertyAccessor)Activator.CreateInstance(typeof(PropertyWrapper<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType), propertyInfo);
        }

        public object Get(object target)
        {
            Guard.NotNull(target, nameof(target));

            return internalAccessor.Get(target);
        }

        public void Set(object target, object value)
        {
            Guard.NotNull(target, nameof(target));

            internalAccessor.Set(target, value);
        }
    }
}
