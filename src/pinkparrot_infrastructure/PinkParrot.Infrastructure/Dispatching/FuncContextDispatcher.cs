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
    public sealed class FuncContextDispatcher<TTarget, TIn, TContext, TOut>
    {
        private static readonly Dictionary<Type, Func<TTarget, object, TContext, TOut>> Handlers;

        static FuncContextDispatcher()
        {
            Handlers =
                typeof(TTarget)
                    .GetMethods()
                    .Where(Helper.HasRightName)
                    .Where(Helper.HasRightParameters<TIn, TContext>)
                    .Where(Helper.HasRightReturnType<TOut>)
                    .Select(FuncContextDispatcherFactory.CreateFuncHandler<TTarget, TContext, TOut >)
                    .ToDictionary(h => h.Item1, h => h.Item2);
        }

        public static TOut Dispatch(TTarget target, TIn item, TContext context)
        {
            Func<TTarget, object, TContext, TOut > handler;

            if (Handlers.TryGetValue(item.GetType(), out handler))
            {
                return handler(target, item, context);
            }

            return default(TOut);
        }
    }
}