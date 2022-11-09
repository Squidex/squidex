// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Web.Pipeline;

public sealed class CachingOptions
{
    public bool StrongETag { get; set; }

    public int MaxSurrogateKeysSize { get; set; } = 8000;
}
