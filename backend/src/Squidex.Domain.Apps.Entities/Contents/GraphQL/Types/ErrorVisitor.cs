// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal static class ErrorVisitor
{
    public static void HandleError(this ExecutionOptions options, IServiceProvider services)
    {
        options.UnhandledExceptionDelegate = context =>
        {
            var log = services.GetRequiredService<ILoggerFactory>().CreateLogger("GraphQL");

            var fieldName = context.FieldContext?.FieldDefinition?.Name;

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                LogMessages.LogFailedToResolveField(log, fieldName, context.OriginalException);
            }
            else
            {
                LogMessages.LogFailedToResolveQuery(log, context.OriginalException);
            }

            if (context.OriginalException is ValidationException or DomainException)
            {
                var message = context.OriginalException.Message;

                context.ErrorMessage = context.OriginalException.Message;
            }

            return Task.CompletedTask;
        };
    }
}
