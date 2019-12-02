// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Lucene.Net.Store;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface IDirectoryFactory
    {
        Directory Create(Guid schemaId);
    }
}
