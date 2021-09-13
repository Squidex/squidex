// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Xml.Linq;
using FakeItEasy;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Users
{
    public sealed class DefaultXmlRepositoryTests
    {
        private readonly ISnapshotStore<DefaultXmlRepository.State> store = A.Fake<ISnapshotStore<DefaultXmlRepository.State>>();
        private readonly DefaultXmlRepository sut;

        public DefaultXmlRepositoryTests()
        {
            sut = new DefaultXmlRepository(store);
        }

        [Fact]
        public void Should_read_from_store()
        {
            A.CallTo(() => store.ReadAllAsync(default))
                .Returns(new[]
                {
                    (new DefaultXmlRepository.State
                    {
                        Xml = new XElement("xml").ToString()
                    }, 0L),
                    (new DefaultXmlRepository.State
                    {
                        Xml = new XElement("xml").ToString()
                    }, 0L)
                }.ToAsyncEnumerable());

            var xml = sut.GetAllElements();

            Assert.Equal(2, xml.Count);
        }

        [Fact]
        public void Should_write_to_store()
        {
            var xml = new XElement("xml");

            sut.StoreElement(xml, "name");

            A.CallTo(() => store.WriteAsync(DomainId.Create("name"), A<DefaultXmlRepository.State>._, A<long>._, 0, default))
                .MustHaveHappened();
        }
    }
}
