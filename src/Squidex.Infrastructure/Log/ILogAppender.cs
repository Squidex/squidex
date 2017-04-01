// ==========================================================================
//  ILogAppender.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Infrastructure.Log
{
    public interface ILogAppender
    {
        void Append(IObjectWriter writer);
    }
}
