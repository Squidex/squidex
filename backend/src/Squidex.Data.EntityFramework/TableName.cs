// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex;

public static class TableName
{
    private static readonly AsyncLocal<string> CurrentPrefix = new AsyncLocal<string>();

    public static string Prefix
    {
        get => CurrentPrefix.Value ?? string.Empty;
        set => CurrentPrefix.Value = value;
    }
}
