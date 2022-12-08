﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Comments;

public sealed record Comment(DomainId Id, Instant Time, RefToken User, string Text, Uri? Url = null)
{
    public RefToken User { get; } = Guard.NotNull(User);

    public string Text { get; } = Guard.NotNullOrEmpty(Text);
}
