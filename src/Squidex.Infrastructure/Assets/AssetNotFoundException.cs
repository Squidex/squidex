// ==========================================================================
//  AssetNotFoundException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Assets
{
    public class AssetNotFoundException : Exception
    {
        public AssetNotFoundException()
        {
        }

        public AssetNotFoundException(string message)
            : base(message)
        {
        }

        public AssetNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
