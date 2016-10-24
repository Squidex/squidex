// ==========================================================================
//  TaskHelper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.Tasks
{
    public static class TaskHelper
    {
        public static readonly Task Done = CreateDoneTask();

        private static Task CreateDoneTask()
        {
            var result = new TaskCompletionSource<object>();

            result.SetResult(null);

            return result.Task;
        }
    }
}