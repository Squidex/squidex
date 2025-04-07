// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO.Compression;

namespace Squidex.Web;

public class CompressionOptions
{
    public bool Enabled { get; set; }

    public bool EnableForHttps { get; set; }

    public CompressionLevel LevelGzip { get; set; } = CompressionLevel.Fastest;

    public CompressionLevel LevelBrotli { get; set; } = CompressionLevel.Fastest;
}
