// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public interface IStatefulObject<TKey>
    {
        Task ActivateAsync(TKey key);
    }
}
