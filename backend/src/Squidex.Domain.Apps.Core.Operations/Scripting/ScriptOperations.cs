// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Scripting
{
    internal static class ScriptOperations
    {
        private delegate void MessageDelegate(string? message);

        private static readonly MessageDelegate Disallow = new MessageDelegate(message =>
        {
            message = !string.IsNullOrWhiteSpace(message) ? message : T.Get("common.jsNotAlloweed");

            throw new DomainForbiddenException(message);
        });

        private static readonly MessageDelegate Reject = new MessageDelegate(message =>
        {
            message = !string.IsNullOrWhiteSpace(message) ? message : T.Get("common.jsRejected");

            throw new ValidationException(message);
        });

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
