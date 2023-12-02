﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas.Json;

public sealed class FieldsSurrogate : List<FieldSurrogate>, ISurrogate<FieldCollection<RootField>>
{
    public void FromSource(FieldCollection<RootField> source)
    {
        foreach (var field in source.Ordered)
        {
            Add(FieldSurrogate.FromSource(field));
        }
    }

    public FieldCollection<RootField> ToSource()
    {
        return new FieldCollection<RootField>(this.Select(x => x.ToField()).ToArray());
    }
}
