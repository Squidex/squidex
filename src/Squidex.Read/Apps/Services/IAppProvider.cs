// ==========================================================================
//  IAppProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace PinkParrot.Read.Apps.Services
{
    public interface IAppProvider
    {
        Task<Guid?> FindAppIdByNameAsync(string name);
    }
}
