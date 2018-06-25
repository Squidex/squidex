// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Contents
{
    public sealed class ContentsControllerOptions
    {
        public bool EnableSurrogateKeys { get; set; }

        public int MaxItemsForSurrogateKeys { get; set; }
    }
}
