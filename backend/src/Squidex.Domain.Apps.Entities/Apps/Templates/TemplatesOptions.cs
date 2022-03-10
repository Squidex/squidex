// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
<<<<<<<< HEAD:backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/TemplatesOptions.cs
    public sealed class TemplatesOptions
    {
        public TemplateRepository[] Repositories { get; set; }
========
    public sealed record Template(string Name, string Title, string Description)
    {
>>>>>>>> a72d238e30f1469baf2db1c468c56df02cb7a70e:backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/Template.cs
    }
}
