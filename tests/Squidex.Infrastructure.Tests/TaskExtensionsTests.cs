// ==========================================================================
//  TaskExtensionsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure
{
    public class TaskExtensionsTests
    {
        [Fact]
        public void Should_do_nothing_on_forget()
        {
            var task = Task.FromResult(123);

            task.Forget();

            Assert.Equal(123, task.Result);
        }
    }
}
