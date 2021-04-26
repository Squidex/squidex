// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Squidex.Web.Pipeline;

namespace Squidex.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ApiCostsAttribute : ServiceFilterAttribute, IApiCostsFeature
    {
        public double Costs { get; }

        public ApiCostsAttribute(double costs)
            : base(typeof(ApiCostsFilter))
        {
            Costs = costs;
        }
    }
}
