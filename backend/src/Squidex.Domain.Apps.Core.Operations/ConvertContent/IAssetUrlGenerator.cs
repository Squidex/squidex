﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public interface IAssetUrlGenerator
    {
        string GenerateUrl(string assetId);
    }
}
