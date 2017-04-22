// ==========================================================================
//  IAssetTester.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Core.Schemas
{
    public interface IAssetTester
    {
        Task<bool> IsValidAsync(Guid assetId);
    }
}
