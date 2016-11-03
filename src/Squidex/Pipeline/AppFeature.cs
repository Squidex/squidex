// ==========================================================================
//  AppFeature.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Read.Apps;

namespace Squidex.Pipeline
{
    public sealed class AppFeature : IAppFeature
    {
        public IAppEntity App { get; }

        public AppFeature(IAppEntity app)
        {
            App = app;
        }
    }
}