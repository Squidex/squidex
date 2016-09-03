// ==========================================================================
//  ActionHandlerFactory.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;

namespace PinkParrot.Infrastructure.Dispatching
{
    internal class ActionDispatcherFactory
    {
        public static Tuple<Type, Action<T, object>> CreateActionHandler<T>(MethodInfo methodInfo)
        {
            var inputType = methodInfo.GetParameters()[0].ParameterType;

            var factoryMethod =
                typeof(ActionDispatcherFactory)
                    .GetMethod("Factory", BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(typeof(T), inputType);

            var handler = factoryMethod.Invoke(null, new object[] { methodInfo });

            return new Tuple<Type, Action<T, object>>(inputType, (Action<T, object>)handler);
        }

        // ReSharper disable once UnusedMember.Local
        private static Action<TTarget, object> Factory<TTarget, TIn>(MethodInfo methodInfo)
        {
            var type = typeof(Action<TTarget, TIn>);

            var handler = (Action<TTarget, TIn>)methodInfo.CreateDelegate(type);

            return (target, input) => handler(target, (TIn)input);
        }
    }
}