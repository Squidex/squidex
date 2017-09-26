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
    public sealed class ActionDispatcher<TTarget, TIn>
    {
        public delegate void ActionDelegate<in T>(TTarget target, T input) where T : TIn;

        public static readonly Func<TTarget, TIn, bool> On = CreateHandler();

        public static Func<TTarget, TIn, bool> CreateHandler(string methodName = "On")
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
                        m.HasMatchingParameters<TIn>() &&
                        m.HasMatchingReturnType(typeof(void)))
                    .Select(m =>
                    {
                        var inputType = m.GetParameters()[0].ParameterType;

                        var handler =
                            typeof(ActionDispatcher<TTarget, TIn>)
                                .GetMethod(nameof(Factory),
                                    BindingFlags.Static |
                                    BindingFlags.NonPublic)
                                .MakeGenericMethod(inputType)
                                .Invoke(null, new object[] { m });

                        return (inputType, handler);
                    })
                    .ToDictionary(m => m.Item1, h => (ActionDelegate<TIn>)h.Item2);

            return (target, input) =>
            {
                if (handlers.TryGetValue(input.GetType(), out var handler))
                {
                    handler(target, input);

                    return true;
                }

                return false;
            };
        }

        private static ActionDelegate<TIn> Factory<T>(MethodInfo methodInfo) where T : TIn
        {
            var handler = (ActionDelegate<T>)methodInfo.CreateDelegate(typeof(ActionDelegate<T>));

            return (target, input) => handler(target, (T)input);
        }
    }
}