// ==========================================================================
//  ActionContextDispatcher.cs
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
    public sealed class ActionContextDispatcher<TTarget, TIn, TContext>
    {
        private static readonly Dictionary<Type, Action<TTarget, object, TContext>> Handlers;

        static ActionContextDispatcher()
        {
            Handlers =
                typeof(TTarget)
                    .GetMethods()
                    .Where(Helper.HasRightName)
                    .Where(Helper.HasRightParameters<TIn, TContext>)
                    .Select(ActionContextDispatcherFactory.CreateActionHandler<TTarget, TContext>)
                    .ToDictionary<Tuple<Type, Action<TTarget, object, TContext>>, Type, Action<TTarget, object, TContext>>(h => h.Item1, h => h.Item2);
        }

        public static bool Dispatch(TTarget target, TIn input, TContext context)
        {
            Action<TTarget, object, TContext> handler;

            if (!Handlers.TryGetValue(input.GetType(), out handler))
            {
                return false;
            }

            handler(target, input, context);

            return true;
        }
    }
}