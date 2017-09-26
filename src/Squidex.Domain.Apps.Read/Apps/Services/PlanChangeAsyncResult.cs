// ==========================================================================
//  PlanChangeAsyncResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Domain.Apps.Read.Apps.Services
{
    public sealed class PlanChangeAsyncResult : IChangePlanResult
    {
        public static readonly PlanChangeAsyncResult Instance = new PlanChangeAsyncResult();

        private PlanChangeAsyncResult()
        {
        }
    }
}
