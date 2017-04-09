// ==========================================================================
//  IConsole.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Infrastructure.Log.Internal
{
    public interface IConsole
    {
        void WriteLine(SemanticLogLevel level, string message);
    }
}
