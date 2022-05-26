// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Dump
{
    public sealed class DumperOptions
    {
        public string? GcDumpTool { get; set; }

        public string? DumpTool { get; set; }

        public int GCDumpTrigger { get; set; }

        public int DumTrigger { get; set; }
    }
}
