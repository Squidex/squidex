// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Tasks
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
        }

        public static Func<TInput, TOutput> ToDefault<TInput, TOutput>(this Action<TInput> action)
        {
            Guard.NotNull(action, nameof(action));

            return x =>
            {
                action(x);

                return default(TOutput);
            };
        }

        public static Func<TInput, Task<TOutput>> ToDefault<TInput, TOutput>(this Func<TInput, Task> action)
        {
            Guard.NotNull(action, nameof(action));

            return async x =>
            {
                await action(x);

                return default(TOutput);
            };
        }

        public static Func<TInput, Task<TOutput>> ToAsync<TInput, TOutput>(this Func<TInput, TOutput> action)
        {
            Guard.NotNull(action, nameof(action));

            return x =>
            {
                var result = action(x);

                return Task.FromResult(result);
            };
        }

        public static Func<TInput, Task> ToAsync<TInput>(this Action<TInput> action)
        {
            return x =>
            {
                action(x);

                return TaskHelper.Done;
            };
        }
    }
}
