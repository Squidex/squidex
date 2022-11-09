// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Diagnostics;

public sealed class DiagnoserOptions
{
    public string? GcDumpTool { get; set; }

    public string? DumpTool { get; set; }

    public int GCDumpTriggerInMB { get; set; }

    public int DumpTriggerInMB { get; set; }
}
