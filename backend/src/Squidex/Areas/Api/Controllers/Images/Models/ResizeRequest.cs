// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;

namespace Squidex.Areas.Api.Controllers.Images.Models
{
    public sealed class ResizeRequest
    {
        public string SourcePath { get; set; }

        public string SourceMimeType { get; set; }

        public string TargetPath { get; set; }

        public bool Overwrite { get; set; }

        public ResizeOptions ResizeOptions { get; set; }
    }
}
