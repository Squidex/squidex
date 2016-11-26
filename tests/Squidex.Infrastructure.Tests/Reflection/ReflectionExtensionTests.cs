// ==========================================================================
//  ReflectionExtensionTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using Xunit;
// ReSharper disable UnusedMember.Local

namespace Squidex.Infrastructure.Reflection
{
    public class ReflectionExtensionTests
    {
        private interface IMain : ISub1
        {
            string MainProp { get; set; }
        }

        private interface ISub1 : ISub2
        {
            string Sub1Prop { get; set; }
        }

        private interface ISub2
        {
            string Sub2Prop { get; set; }
        }

        private class Main
        {
            public string MainProp { get; set; }
        }

        [Fact]
        public void Should_find_all_public_properties_of_interfaces()
        {
            var properties = typeof(IMain).GetPublicProperties().Select(x => x.Name).OrderBy(x => x).ToArray();

            Assert.Equal(new [] { "MainProp", "Sub1Prop", "Sub2Prop" }, properties);
        }

        [Fact]
        public void Should_find_all_public_properties_of_classes()
        {
            var properties = typeof(Main).GetPublicProperties().Select(x => x.Name).OrderBy(x => x).ToArray();

            Assert.Equal(new[] { "MainProp" }, properties);
        }
    }
}
