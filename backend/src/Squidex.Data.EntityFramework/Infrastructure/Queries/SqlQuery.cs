// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public class SqlQuery(string table)
{
    public string Table => table;

    public bool AsCount { get; set; }

    public long Limit { get; set; }

    public long Offset { get; set; }

    public List<string> Fields { get; set; } = ["*"];

    public List<string> Where { get; set; } = [];

    public List<string> Order { get; set; } = [];
}
