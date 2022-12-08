// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;
using Xunit.Sdk;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public static class ValidationAssert
{
    public static void Throws(Action action, params ValidationError[] errors)
    {
        try
        {
            action();

            Assert.True(false, $"Expected {typeof(ValidationException)} but succeeded");
        }
        catch (ValidationException ex)
        {
            ex.Errors.ToArray().Should().BeEquivalentTo(errors);
        }
        catch (XunitException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Assert.True(false, $"Excepted {typeof(ValidationException)}, but got {ex.GetType()}");
        }
    }

    public static async Task ThrowsAsync(Func<Task> action, params ValidationError[] errors)
    {
        try
        {
            await action();

            Assert.True(false, $"Expected {typeof(ValidationException)} but succeeded");
        }
        catch (ValidationException ex)
        {
            ex.Errors.ToArray().Should().BeEquivalentTo(errors);
        }
        catch (XunitException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Assert.True(false, $"Excepted {typeof(ValidationException)}, but got {ex.GetType()}");
        }
    }
}
