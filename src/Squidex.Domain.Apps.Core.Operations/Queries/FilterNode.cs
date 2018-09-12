// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Queries
{
    public abstract class FilterNode
    {
        public abstract T Accept<T>(FilterNodeVisitor<T> visitor);
    }
}
