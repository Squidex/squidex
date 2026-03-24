// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules;

public sealed class ExpressionsAttribute(string? interpolationOld, string? interpolationNew, string? script, string? liquid) : DataAttribute
{
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
    {
        var rows = new List<ITheoryDataRow>();

        if (interpolationOld != null)
        {
            rows.Add(new TheoryDataRow(interpolationOld));
        }

        if (interpolationNew != null)
        {
            rows.Add(new TheoryDataRow(interpolationNew));
        }

        if (script != null)
        {
            rows.Add(new TheoryDataRow($"Script(`{script}`)"));
        }

        if (liquid != null)
        {
            rows.Add(new TheoryDataRow($"Liquid({liquid})"));
        }

        return new ValueTask<IReadOnlyCollection<ITheoryDataRow>>(rows);
    }

    public override bool SupportsDiscoveryEnumeration()
    {
        return true;
    }
}
