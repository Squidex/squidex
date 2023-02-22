// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Scripting;

internal static class ScriptOperations
{
    private delegate void MessageDelegate(string? message);

    private delegate void MessageJsonDelegate(string? message);

    private static readonly Action<string> Disallow = message =>
    {
        message = !string.IsNullOrWhiteSpace(message) ? message : T.Get("common.jsNotAllowed");

        throw new DomainForbiddenException(message);
    };

    private static readonly Action<JsValue> Reject = message =>
    {
        var errors = new List<ValidationError>();

        void AddError(JsString message)
        {
            var text = message.ToString();

            if (!string.IsNullOrWhiteSpace(text))
            {
                errors.Add(new ValidationError(text));
            }
        }

        if (message is JsString typed)
        {
            AddError(typed);
        }
        else if (message is JsArray jsArray)
        {
            foreach (var item in jsArray)
            {
                if (item is JsString typedItem)
                {
                    AddError(typedItem);
                }
            }
        }

        if (errors.Count == 0)
        {
            errors.Add(new ValidationError(T.Get("common.jsRejected")));
        }

        throw new ValidationException(errors);
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
