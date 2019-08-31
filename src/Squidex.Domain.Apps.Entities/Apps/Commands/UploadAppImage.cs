// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UploadAppImage : AppCommand
    {
        public Func<Stream> File { get; set; }
    }
}
