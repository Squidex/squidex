// ==========================================================================
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
        private delegate void MessageDelegate(string? message);

        private static readonly MessageDelegate Disallow = message =>
        {
            message = !string.IsNullOrWhiteSpace(message) ? message : "Not allowed";

            throw new DomainForbiddenException(message);
        };

        private static readonly MessageDelegate Reject = message =>
        {
            var errors = !string.IsNullOrWhiteSpace(message) ? new[] { new ValidationError(message) } : null;

            throw new ValidationException("Script rejected the operation.", errors);
        };

        public static Engine AddDisallow(this Engine engine)
        {
            engine.SetValue("disallow", Disallow);

            return engine;
        }

        public static Engine AddReject(this Engine engine)
        {
            engine.SetValue("reject", Reject);

            return engine;
        }
    }
}
