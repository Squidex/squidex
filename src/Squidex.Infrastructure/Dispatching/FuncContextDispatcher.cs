// ==========================================================================
//  FuncContextDispatcher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Reflection;

namespace Squidex.Infrastructure.Dispatching
{
    public sealed class FuncContextDispatcher<TTarget, TIn, TContext, TOut>
    {
        public delegate TOut FuncContextDelegate<in T>(TTarget target, T input, TContext context) where T : TIn;

        public static readonly FuncContextDelegate<TIn> On = CreateHandler();

        public static FuncContextDelegate<TIn> CreateHandler(string methodName = "On")
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
                        m.HasMatchingReturnType(typeof(TOut)))
                    .Select(m =>
                    {
                        var inputType = m.GetParameters()[0].ParameterType;

                        var handler =
                            typeof(FuncContextDispatcher<TTarget, TIn, TContext, TOut>)
                                .GetMethod(nameof(Factory),
                                    BindingFlags.Static |
                                    BindingFlags.NonPublic)
                                .MakeGenericMethod(inputType)
                                .Invoke(null, new object[] { m });

                        return (inputType, handler);
                    })
                    .ToDictionary(m => m.Item1, h => (FuncContextDelegate<TIn>)h.Item2);

            return (target, input, context) => handlers.TryGetValue(input.GetType(), out var handler) ? handler(target, input, context) : default(TOut);
        }

        private static FuncContextDelegate<TIn> Factory<T>(MethodInfo methodInfo) where T : TIn
        {
            var handler = (FuncContextDelegate<T>)methodInfo.CreateDelegate(typeof(FuncContextDelegate<T>));

            return (target, input, context) => handler(target, (T)input, context);
        }
    }
}