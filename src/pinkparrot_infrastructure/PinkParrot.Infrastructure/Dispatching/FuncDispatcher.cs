// ==========================================================================
//  FuncDispatcher.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PinkParrot.Infrastructure.Dispatching
{
    public sealed class FuncDispatcher<TTarget, TIn, TOut>
    {
        private static readonly Dictionary<Type, Func<TTarget, object, TOut>> Handlers;

        static FuncDispatcher()
        {
            Handlers =
                typeof(TTarget)
                    .GetMethods()
                    .Where(Helper.HasRightName)
                    .Where(Helper.HasRightParameters<TIn>)
                    .Where(Helper.HasRightReturnType<TOut>)
                    .Select(FuncDispatcherFactory.CreateFuncHandler<TTarget, TOut>)
                    .ToDictionary<Tuple<Type, Func<TTarget, object, TOut>>, Type, Func<TTarget, object, TOut>>(h => h.Item1, h => h.Item2);
        }

        public static TOut Dispatch(TTarget target, TIn item)
        {
            Func<TTarget, object, TOut> handler;

            if (Handlers.TryGetValue(item.GetType(), out handler))
            {
                return handler(target, item);
            }

            return default(TOut);
        }
    }
}