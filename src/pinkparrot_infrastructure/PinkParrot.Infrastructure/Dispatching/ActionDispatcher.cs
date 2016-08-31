// ==========================================================================
//  ActionDispatcher.cs
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
    public sealed class ActionDispatcher<TTarget, TIn>
    {
        private static readonly Dictionary<Type, Action<TTarget, object>> Handlers;

        static ActionDispatcher()
        {
            Handlers =
                typeof(TTarget)
                    .GetMethods()
                    .Where(Helper.HasRightName)
                    .Where(Helper.HasRightParameters<TIn>)
                    .Select(ActionDispatcherFactory.CreateActionHandler<TTarget>)
                    .ToDictionary(h => h.Item1, h => h.Item2);
        }

        public static bool Dispatch(TTarget target, TIn item)
        {
            Action<TTarget, object> handler;

            if (!Handlers.TryGetValue(item.GetType(), out handler))
            {
                return false;
            }

            handler(target, item);

            return true;
        }
    }
}