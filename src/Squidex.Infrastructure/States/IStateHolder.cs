// ==========================================================================
//  IStateHolder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public interface IStateHolder<T>
    {
        T State { get; set; }

        Task ReadAsync();

        Task WriteAsync();
    }
}
