// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Pipeline
{
    public interface IGenerateEtag
    {
        Guid Id { get; }

        long Version { get; }
    }
}
