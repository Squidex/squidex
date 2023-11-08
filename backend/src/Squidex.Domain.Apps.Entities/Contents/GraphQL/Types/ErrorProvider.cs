// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Execution;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

public sealed class ErrorProvider : ErrorInfoProvider
{
    public override ErrorInfo GetInfo(ExecutionError executionError)
    {
        var actual = base.GetInfo(executionError);

        if (executionError.InnerException is ValidationException or DomainException)
        {
            if (!string.IsNullOrWhiteSpace(actual.Message))
            {
                actual.Message = $"{actual.Message} - {executionError.InnerException.Message}";
            }
            else
            {
                actual.Message = executionError.InnerException.Message;
            }
        }

        return actual;
    }
}
