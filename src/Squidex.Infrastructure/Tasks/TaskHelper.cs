// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.Tasks
{
    public static class TaskHelper
    {
        public static readonly Task Done = CreateDoneTask();
        public static readonly Task<bool> False = CreateResultTask(false);
        public static readonly Task<bool> True = CreateResultTask(true);

        private static Task CreateDoneTask()
        {
            var result = new TaskCompletionSource<object>();

            result.SetResult(null);

            return result.Task;
        }

        private static Task<bool> CreateResultTask(bool value)
        {
            var result = new TaskCompletionSource<bool>();

            result.SetResult(value);

            return result.Task;
        }
    }
}