// ==========================================================================
//  AggregateHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.TestHelpers
{
    internal sealed class MyEvent : IEvent
    {
        public string MyProperty { get; set; }
    }
}