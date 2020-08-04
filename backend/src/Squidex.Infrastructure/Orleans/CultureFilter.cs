// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class CultureFilter : IIncomingGrainCallFilter, IOutgoingGrainCallFilter
    {
        public Task Invoke(IOutgoingGrainCallContext context)
        {
            RequestContext.Set("Culture", CultureInfo.CurrentCulture.Name);
            RequestContext.Set("CultureUI", CultureInfo.CurrentUICulture.Name);

            return context.Invoke();
        }

        public Task Invoke(IIncomingGrainCallContext context)
        {
            if (RequestContext.Get("Culture") is string culture)
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            }

            if (RequestContext.Get("CultureUI") is string cultureUI)
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureUI);
            }

            return context.Invoke();
        }
    }
}
