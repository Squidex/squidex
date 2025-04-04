﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Xunit.Sdk;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules;

public sealed class ExpressionsAttribute(string? interpolationOld, string? interpolationNew, string? script, string? liquid) : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        if (interpolationOld != null)
        {
            yield return new object[] { interpolationOld };
        }

        if (interpolationNew != null)
        {
            yield return new object[] { interpolationNew };
        }

        if (script != null)
        {
            yield return new object[]
            {
                $"Script(`{script}`)",
            };
        }

        if (liquid != null)
        {
            yield return new object[]
            {
                $"Liquid({liquid})",
            };
        }
    }
}
