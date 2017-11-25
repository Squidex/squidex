// ==========================================================================
//  ReadSchemaState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Read.State.Orleans.Grains;
using Squidex.Infrastructure.States;

namespace Benchmarks.Tests.TestData
{
    public sealed class MyAppState : StatefulObject<AppStateGrainState>
    {
        public void SetState(AppStateGrainState state)
        {
            State = state;
        }
    }
}
