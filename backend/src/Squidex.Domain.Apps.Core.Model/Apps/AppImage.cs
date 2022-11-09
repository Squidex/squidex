// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Apps;

public sealed record AppImage(string MimeType, string? Etag = null)
{
    public string MimeType { get; } = Guard.NotNullOrEmpty(MimeType);

    public string Etag { get; } = Etag ?? RandomHash.Simple();
}
