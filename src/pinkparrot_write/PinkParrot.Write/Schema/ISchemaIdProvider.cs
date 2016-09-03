// ==========================================================================
//  ISchemaIdProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace PinkParrot.Write.Schema
{
    public interface ISchemaIdProvider
    {
        Task<Guid> FindSchemaId(string name);
    }
}
