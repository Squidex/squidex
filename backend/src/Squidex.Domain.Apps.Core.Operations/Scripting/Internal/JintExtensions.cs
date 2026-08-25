// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

public static class JintExtensions
{
    public static List<DomainId> ToIds(this JsValue? value)
    {
        var ids = new List<DomainId>();

        if (value is JsString s)
        {
            ids.Add(DomainId.Create(s.AsString()));
        }
        else if (value is JsArray a)
        {
            foreach (var item in a.OfType<JsString>())
            {
                ids.Add(DomainId.Create(item.AsString()));
            }
        }

        return ids;
    }
}
