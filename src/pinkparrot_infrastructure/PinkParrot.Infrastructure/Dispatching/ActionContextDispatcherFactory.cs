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
    internal class ActionContextDispatcherFactory
    {
        public static Tuple<Type, Action<TTarget, object, TContext>> CreateActionHandler<TTarget, TContext>(MethodInfo methodInfo)
        {
            var inputType = methodInfo.GetParameters()[0].ParameterType;

            var factoryMethod =
                typeof(ActionContextDispatcherFactory)
                    .GetMethod("Factory", BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(typeof(TTarget), inputType, typeof(TContext));

            var handler = factoryMethod.Invoke(null, new object[] { methodInfo });

            return new Tuple<Type, Action<TTarget, object, TContext>>(inputType, (Action<TTarget, object, TContext>)handler);
        }

        // ReSharper disable once UnusedMember.Local
        private static Action<TTarget, object, TContext> Factory<TTarget, TIn, TContext>(MethodInfo methodInfo)
        {
            var type = typeof(Action<TTarget, TIn, TContext>);

            var handler = (Action<TTarget, TIn, TContext>)methodInfo.CreateDelegate(type);

            return (target, input, context) => handler(target, (TIn)input, context);
        }
    }
}