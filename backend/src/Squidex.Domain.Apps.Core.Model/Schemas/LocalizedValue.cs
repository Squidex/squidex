// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed class LocalizedValue<T> : ReadonlyDictionary<string, T>
{
    public LocalizedValue()
    {
    }

    public LocalizedValue(IDictionary<string, T> inner)
        : base(inner)
    {
    }
}
