// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure;

public class TaskExtensionsTests
{
    [Fact]
    public void Should_do_nothing_on_forget()
    {
        var task = Task.FromResult(123);

        task.Forget();

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        Assert.Equal(123, task.Result);
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
    }
}
