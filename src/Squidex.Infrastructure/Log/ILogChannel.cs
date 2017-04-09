// ==========================================================================
//  ILogChannel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Infrastructure.Log
{
    public interface ILogChannel
    {
        void Log(SemanticLogLevel logLevel, string message);
    }
}