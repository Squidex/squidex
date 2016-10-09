// ==========================================================================
//  DispatchExtensions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace PinkParrot.Infrastructure.Dispatching
{
    public static class DispatchExtensions
    {
        public static bool DispatchAction<TTarget, TIn>(this TTarget target, TIn input)
        {
            return ActionDispatcher<TTarget, TIn>.Dispatch(target, input);
        }

        public static bool DispatchAction<TTarget, TIn, TContext>(this TTarget target, TIn input, TContext context)
        {
            return ActionContextDispatcher<TTarget, TIn, TContext>.Dispatch(target, input, context);
        }

        public static async Task<bool> DispatchActionAsync<TTarget, TIn>(this TTarget target, TIn input)
        {
            var task = FuncDispatcher<TTarget, TIn, Task>.Dispatch(target, input);

            if (task == null)
            {
                return false;
            }

            await task;

            return true;
        }

        public static async Task<bool> DispatchActionAsync<TTarget, TIn, TContext>(this TTarget target, TIn input, TContext context)
        {
            var task = FuncContextDispatcher<TTarget, TIn, TContext, Task>.Dispatch(target, input, context);

            if (task == null)
            {
                return false;
            }

            await task;

            return true;
        }

        public static TOut DispatchFunc<TTarget, TIn, TOut>(this TTarget target, TIn input, TOut fallback)
        {
            var result = FuncDispatcher<TTarget, TIn, TOut>.Dispatch(target, input);

            return Equals(result, default(TOut)) ? fallback : result;
        }

        public static TOut DispatchFunc<TTarget, TIn, TContext, TOut>(this TTarget target, TIn input, TContext context, TOut fallback)
        {
            var result = FuncContextDispatcher<TTarget, TIn, TContext, TOut>.Dispatch(target, input, context);

            return Equals(result, default(TOut)) ? fallback : result;
        }

        public static Task<TOut> DispatchFuncAsync<TTarget, TIn, TOut>(this TTarget target, TIn input, TOut fallback)
        {
            var result = FuncDispatcher<TTarget, TIn, Task<TOut>>.Dispatch(target, input);

            return result ?? Task.FromResult(fallback);
        }

        public static Task<TOut> DispatchFuncAsync<TTarget, TIn, TContext, TOut>(this TTarget target, TIn input, TContext context, TOut fallback)
        {
            var result = FuncContextDispatcher<TTarget, TIn, TContext, Task<TOut>>.Dispatch(target, input, context);

            return result ?? Task.FromResult(fallback);
        }
    }
}