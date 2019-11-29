// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using FakeItEasy;
using Orleans;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class DomainObjectGrainFormatterTests
    {
        private readonly IIncomingGrainCallContext context = A.Fake<IIncomingGrainCallContext>();

        [Fact]
        public void Should_return_fallback_if_no_method_is_defined()
        {
            A.CallTo(() => context.InterfaceMethod)
                .Returns(null!);

            var result = DomainObjectGrainFormatter.Format(context);

            Assert.Equal("Unknown", result);
        }

        [Fact]
        public void Should_return_method_name_if_not_domain_object_method()
        {
            var methodInfo = A.Fake<MethodInfo>();

            A.CallTo(() => methodInfo.Name)
                .Returns("Calculate");

            A.CallTo(() => context.InterfaceMethod)
                .Returns(methodInfo);

            var result = DomainObjectGrainFormatter.Format(context);

            Assert.Equal("Calculate", result);
        }

        [Fact]
        public void Should_return_nice_method_name_if_domain_object_execute()
        {
            var methodInfo = A.Fake<MethodInfo>();

            A.CallTo(() => methodInfo.Name)
                .Returns(nameof(IDomainObjectGrain.ExecuteAsync));

            A.CallTo(() => context.Arguments)
                .Returns(new object[] { new MyCommand() });

            A.CallTo(() => context.InterfaceMethod)
                .Returns(methodInfo);

            var result = DomainObjectGrainFormatter.Format(context);

            Assert.Equal("ExecuteAsync(MyCommand)", result);
        }
    }
}
