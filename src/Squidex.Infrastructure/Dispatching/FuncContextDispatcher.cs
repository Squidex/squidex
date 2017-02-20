// ==========================================================================
//  FuncContextDispatcher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Squidex.Infrastructure.Dispatching
{
    public sealed class FuncContextDispatcher<TTarget, TIn, TContext, TOut>
    {
        private static readonly Dictionary<Type, Func<TTarget, object, TContext, TOut>> Handlers;

        static FuncContextDispatcher()
        {
            Handlers =
                typeof(TTarget)
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(Helper.HasRightName)
                    .Where(Helper.HasRightParameters<TIn, TContext>)
                    .Where(Helper.HasRightReturnType<TOut>)
                    .Select(FuncContextDispatcherFactory.CreateFuncHandler<TTarget, TContext, TOut >)
                    .ToDictionary(h => h.Item1, h => h.Item2);
        }

        public static TOut Dispatch(TTarget target, TIn item, TContext context)
        {
            return Handlers.TryGetValue(item.GetType(), out Func<TTarget, object, TContext, TOut> handler) ? handler(target, item, context) : default(TOut);
        }
    }
}