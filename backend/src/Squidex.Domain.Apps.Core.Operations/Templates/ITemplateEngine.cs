// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.Templates
{
    public interface ITemplateEngine
    {
        Task<string> RenderAsync(string template, TemplateVars variables);
    }
}
