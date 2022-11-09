// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.ValidateContent;

public static class Undefined
{
    public static readonly object Value = new object();

    public static bool IsUndefined(this object? other)
    {
        return ReferenceEquals(other, Value);
    }

    public static bool IsNullOrUndefined(this object? other)
    {
        return other == null || other.IsUndefined();
    }
}
