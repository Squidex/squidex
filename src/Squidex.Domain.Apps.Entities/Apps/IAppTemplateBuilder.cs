// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppTemplateBuilder
    {
        Task PopulateTemplate(IAppEntity app, string name, ICommandBus bus);
    }
}
