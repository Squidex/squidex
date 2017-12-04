// ==========================================================================
//  MyEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Benchmarks.Tests.TestData
{
    [TypeName("MyEvent")]
    public sealed class MyEvent : IEvent
    {
        public int EventNumber { get; set; }
    }
}