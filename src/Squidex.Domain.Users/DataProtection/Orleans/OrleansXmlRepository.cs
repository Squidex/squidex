// ==========================================================================
//  OrleansXmlRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Orleans;
using Squidex.Domain.Users.DataProtection.Orleans.Grains;
using Squidex.Infrastructure;

namespace Squidex.Domain.Users.DataProtection.Orleans
{
    public sealed class OrleansXmlRepository : IXmlRepository
    {
        private readonly Lazy<IXmlRepositoryGrain> grain;

        public OrleansXmlRepository(IClusterClient orleans)
        {
            Guard.NotNull(orleans, nameof(orleans));

            grain = new Lazy<IXmlRepositoryGrain>(() => orleans.GetGrain<IXmlRepositoryGrain>("Default"));
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return grain.Value.GetAllElementsAsync().ContinueWith(x => x.Result.Select(XElement.Parse).ToList()).Result;
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            grain.Value.StoreElementAsync(element.ToString(), friendlyName).Wait();
        }
    }
}
