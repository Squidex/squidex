﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Users;

public sealed class DefaultXmlRepository(ISnapshotStore<DefaultXmlRepository.State> store) : IXmlRepository
{
    [CollectionName("Identity_Xml")]
    public sealed class State
    {
        public string Xml { get; set; }

        public State()
        {
        }

        public State(XElement xml)
        {
            Xml = xml.ToString();
        }

        public XElement ToXml()
        {
            return XElement.Parse(Xml);
        }
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return GetAllElementsAsync().Result;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var state = new State(element);

#pragma warning disable MA0134 // Observe result of async calls
        store.WriteAsync(new SnapshotWriteJob<State>(DomainId.Create(friendlyName), state, 0));
#pragma warning restore MA0134 // Observe result of async calls
    }

    private async Task<IReadOnlyCollection<XElement>> GetAllElementsAsync()
    {
        return await store.ReadAllAsync().Select(x => x.Value.ToXml()).ToListAsync();
    }
}
