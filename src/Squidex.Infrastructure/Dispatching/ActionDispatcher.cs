// ==========================================================================
//  ActionDispatcher.cs
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
    public sealed class ActionDispatcher<TTarget, TIn>
    {
        private static readonly Dictionary<Type, Action<TTarget, object>> Handlers;

        static ActionDispatcher()
        {
            Handlers =
                typeof(TTarget)
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(Helper.HasRightName)
                    .Where(Helper.HasRightParameters<TIn>)
                    .Select(ActionDispatcherFactory.CreateActionHandler<TTarget>)
                    .ToDictionary(h => h.Item1, h => h.Item2);
        }

        public static bool Dispatch(TTarget target, TIn item)
        {
            if (!Handlers.TryGetValue(item.GetType(), out Action<TTarget, object> handler))
            {
                return false;
            }

            handler(target, item);

            return true;
        }
    }
}