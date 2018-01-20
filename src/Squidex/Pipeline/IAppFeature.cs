// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Pipeline
{
    public interface IAppFeature
    {
        IAppEntity App { get; }
    }
}
