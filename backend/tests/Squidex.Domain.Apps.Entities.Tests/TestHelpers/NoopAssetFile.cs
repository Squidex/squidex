﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public sealed class NoopAssetFile(string fileName = "image.png", string mimeType = "image/png", long fileSize = 1024) : DelegateAssetFile(fileName, mimeType, fileSize, () => new MemoryStream())
{
}
