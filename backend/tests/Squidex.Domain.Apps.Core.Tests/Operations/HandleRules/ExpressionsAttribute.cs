// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules
{
    public sealed class ExpressionsAttribute : DataAttribute
    {
        private readonly string? script;
        private readonly string? interpolationOld;
        private readonly string? interpolationNew;
        private readonly string? liquid;

        public ExpressionsAttribute(string? interpolationOld, string? interpolationNew, string? script, string? liquid)
        {
            this.liquid = liquid;

            this.interpolationOld = interpolationOld;
            this.interpolationNew = interpolationNew;

            this.script = script;
        }

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
                    $"Script(`{script}`)"
                };
            }

            if (liquid != null)
            {
                yield return new object[]
                {
                    $"Liquid({liquid})"
                };
            }
        }
    }
}
