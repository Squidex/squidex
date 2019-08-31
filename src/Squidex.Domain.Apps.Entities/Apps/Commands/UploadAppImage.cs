// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UploadAppImage : AppCommand
    {
        public AppImage Image { get; set; }

        public Func<Stream> File { get; set; }
    }
}
