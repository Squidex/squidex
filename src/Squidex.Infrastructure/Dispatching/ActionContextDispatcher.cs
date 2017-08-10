// ==========================================================================
//  FuncDispatcher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Reflection;

namespace Squidex.Infrastructure.Dispatching
{
    public sealed class ActionContextDispatcher<TTarget, TIn, TContext>
    {
        public delegate void ActionContextDelegate<in T>(TTarget target, T input, TContext context) where T : TIn;

        public static readonly Func<TTarget, TIn, TContext, bool> On = CreateHandler();

        public static Func<TTarget, TIn, TContext, bool> CreateHandler(string methodName = "On")
        {
            Guard.NotNullOrEmpty(methodName, nameof(methodName));

            var handlers =
                typeof(TTarget)
                    .GetMethods(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance)
                    .Where(m =>
                        m.HasMatchingName(methodName) &&
                        m.HasMatchingParameters<TIn, TContext>() &&
                        m.HasMatchingReturnType(typeof(void)))
                    .Select(m =>
                    {
                        var inputType = m.GetParameters()[0].ParameterType;

                        var handler =
                            typeof(ActionContextDispatcher<TTarget, TIn, TContext>)
                                .GetMethod(nameof(Factory),
                                    BindingFlags.Static |
                                    BindingFlags.NonPublic)
                                .MakeGenericMethod(inputType)
                                .Invoke(null, new object[] { m });

                        return (inputType, handler);
                    })
                    .ToDictionary(m => m.Item1, h => (ActionContextDelegate<TIn>)h.Item2);

            return (target, input, context) =>
            {
                if (handlers.TryGetValue(input.GetType(), out var handler))
                {
                    handler(target, input, context);

                    return true;
                }

                return false;
            };
        }

        private static ActionContextDelegate<TIn> Factory<T>(MethodInfo methodInfo) where T : TIn
        {
            var handler = (ActionContextDelegate<T>)methodInfo.CreateDelegate(typeof(ActionContextDelegate<T>));

            return (target, input, context) => handler(target, (T)input, context);
        }
    }
}