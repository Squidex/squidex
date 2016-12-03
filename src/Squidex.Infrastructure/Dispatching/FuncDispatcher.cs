// ==========================================================================
//  FuncDispatcher.cs
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
    public sealed class FuncDispatcher<TTarget, TIn, TOut>
    {
        private static readonly Dictionary<Type, Func<TTarget, object, TOut>> Handlers;

        static FuncDispatcher()
        {
            Handlers =
                typeof(TTarget)
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(Helper.HasRightName)
                    .Where(Helper.HasRightParameters<TIn>)
                    .Where(Helper.HasRightReturnType<TOut>)
                    .Select(FuncDispatcherFactory.CreateFuncHandler<TTarget, TOut>)
                    .ToDictionary(h => h.Item1, h => h.Item2);
        }

        public static TOut Dispatch(TTarget target, TIn item)
        {
            Func<TTarget, object, TOut> handler;

            return Handlers.TryGetValue(item.GetType(), out handler) ? handler(target, item) : default(TOut);
        }
    }
}