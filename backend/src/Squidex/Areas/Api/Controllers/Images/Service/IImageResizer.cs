// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Images.Models;

namespace Squidex.Areas.Api.Controllers.Images.Service
{
    public interface IImageResizer
    {
        Task<string> ResizeAsync(ResizeRequest request,
            CancellationToken ct = default);
    }
}
