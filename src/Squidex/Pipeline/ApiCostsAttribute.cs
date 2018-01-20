// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;

namespace Squidex.Pipeline
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ApiCostsAttribute : ServiceFilterAttribute
    {
        public double Weight { get; }

        public ApiCostsAttribute(double weight)
            : base(typeof(ApiCostsFilter))
        {
            Weight = weight;
        }
    }
}
