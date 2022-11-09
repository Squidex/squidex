// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public static class JintExtensions
{
    public static List<DomainId> ToIds(this JsValue? value)
    {
        var ids = new List<DomainId>();

        if (value?.IsString() == true)
        {
            ids.Add(DomainId.Create(value.ToString()));
        }
        else if (value?.IsArray() == true)
        {
            foreach (var item in value.AsArray())
            {
                if (item.IsString())
                {
                    ids.Add(DomainId.Create(item.ToString()));
                }
            }
        }

        return ids;
    }
}
