// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;

namespace Squidex.Infrastructure.Collections;

public class ReadonlyList<T> : ReadOnlyCollection<T>, IEquatable<ReadonlyList<T>>
{
    private static readonly List<T> EmptyInner = new List<T>();

    public ReadonlyList()
        : base(EmptyInner)
    {
    }

    public ReadonlyList(IList<T> list)
        : base(list)
    {
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ReadonlyList<T>);
    }

    public virtual bool Equals(ReadonlyList<T>? other)
    {
        return this.EqualsList(other);
    }

    public override int GetHashCode()
    {
        return this.SequentialHashCode();
    }
}
