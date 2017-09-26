// ==========================================================================
//  FuncDispatcher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Reflection;

namespace Squidex.Infrastructure.Dispatching
{
    public sealed class FuncDispatcher<TTarget, TIn, TOut>
    {
        public delegate TOut FuncDelegate<in T>(TTarget target, T input) where T : TIn;

        public static readonly FuncDelegate<TIn> On = CreateHandler();

        public static FuncDelegate<TIn> CreateHandler(string methodName = "On")
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
                        m.HasMatchingReturnType(typeof(TOut)))
                    .Select(m =>
                    {
                        var inputType = m.GetParameters()[0].ParameterType;

                        var handler =
                            typeof(FuncDispatcher<TTarget, TIn, TOut>)
                                .GetMethod(nameof(Factory),
                                    BindingFlags.Static |
                                    BindingFlags.NonPublic)
                                .MakeGenericMethod(inputType)
                                .Invoke(null, new object[] { m });

                        return (inputType, handler);
                    })
                    .ToDictionary(m => m.Item1, h => (FuncDelegate<TIn>)h.Item2);

            return (target, input) => handlers.TryGetValue(input.GetType(), out var handler) ? handler(target, input) : default(TOut);
        }

        private static FuncDelegate<TIn> Factory<T>(MethodInfo methodInfo) where T : TIn
        {
            var handler = (FuncDelegate<T>)methodInfo.CreateDelegate(typeof(FuncDelegate<T>));

            return (target, input) => handler(target, (T)input);
        }
    }
}