// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public interface ITemplate
    {
        string Name { get; }

        Task RunAsync(PublishTemplate publish);
    }
}
