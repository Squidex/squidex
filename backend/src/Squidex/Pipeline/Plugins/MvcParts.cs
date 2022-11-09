// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Squidex.Pipeline.Plugins;

public static class MvcParts
{
    public static void AddParts(this Assembly assembly, IMvcBuilder mvcBuilder)
    {
        mvcBuilder.ConfigureApplicationPartManager(manager =>
        {
            var parts = manager.ApplicationParts;

            AddParts(parts, assembly);

            foreach (var reference in RelatedAssemblyAttribute.GetRelatedAssemblies(assembly, false))
            {
                AddParts(parts, reference);
            }
        });
    }

    private static void AddParts(IList<ApplicationPart> applicationParts, Assembly assembly)
    {
        var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);

        foreach (var part in partFactory.GetApplicationParts(assembly))
        {
            var existings = applicationParts.Where(x => x.Name == part.Name).ToList();

            foreach (var existing in existings)
            {
                applicationParts.Remove(existing);
            }

            applicationParts.Add(part);
        }
    }
}
