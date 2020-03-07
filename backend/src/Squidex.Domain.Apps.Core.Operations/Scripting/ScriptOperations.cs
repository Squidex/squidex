﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Scripting
{
    internal static class ScriptOperations
    {
        public static ExecutionContext AddDisallow(this ExecutionContext context)
        {
            context.Engine.SetValue("disallow", new DisallowDelegate(Disallow));

            return context;
        }

        private delegate void DisallowDelegate(string? message);

        private static void Disallow(string? message = null)
        {
            message = !string.IsNullOrWhiteSpace(message) ? message : "Not allowed";

            throw new DomainForbiddenException(message);
        }

        public static ExecutionContext AddReject(this ExecutionContext context)
        {
            context.Engine.SetValue("reject", new RejectDelegate(Reject));

            return context;
        }

        private delegate void RejectDelegate(string? message);

        private static void Reject(string? message = null)
        {
            var errors = !string.IsNullOrWhiteSpace(message) ? new[] { new ValidationError(message) } : null;

            throw new ValidationException("Script rejected the operation.", errors);
        }
    }
}
